using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public sealed class TlgInputsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), ITlgInputsRepository
{
    private const string GetAllBaseQuery = """
                                           SELECT
                                               
                                           r.token AS role_token, 
                                           r.role_type AS role_type, 
                                           r.status AS role_status, 
                                               
                                           le.name AS live_event_name, 
                                           le.start_date AS live_event_start_date, 
                                           le.end_date AS live_event_end_date, 
                                           le.status AS live_event_status, 
                                               
                                           dws.resultant_workflow AS input_workflow,
                                           dws.in_state AS input_wf_state,

                                           inp.user_id AS input_user_id, 
                                           inp.chat_id AS input_chat_id, 
                                           inp.interaction_mode AS input_mode, 
                                           inp.input_type AS input_type,
                                           inp.details AS input_details,
                                           inp.entity_guid AS input_guid
                                               
                                           FROM tlg_inputs inp 
                                           LEFT JOIN roles r on inp.role_id = r.id 
                                           LEFT JOIN live_events le on inp.live_event_id = le.id
                                           LEFT JOIN derived_workflow_states dws on dws.tlg_inputs_id = inp.id
                                           """;

    private const string OrderByClause = "ORDER BY inp.id";
    
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Dictionary<TlgAgent, List<TlgInput>> _cacheInputsByTlgAgent = new();
    private Dictionary<ILiveEventInfo, List<TlgInput>> _cacheInputsByLiveEvent = new();
    
    public async Task AddAsync(TlgInput tlgInput) =>
        await AddAsync(new [] { tlgInput });

    public async Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs)
    {
        const string baseQuery = """
                                 INSERT INTO tlg_inputs 
                                 
                                 (user_id, 
                                 chat_id, 
                                 details, 
                                 last_data_migration, 
                                 interaction_mode, 
                                 input_type, 
                                 role_id, 
                                 live_event_id,
                                 entity_guid) 
                                 
                                 VALUES (@tlgUserId, @tlgChatId, @tlgMessageDetails, 
                                 @lastDataMig, @interactionMode, @tlgInputType, 
                                 (SELECT id FROM roles WHERE token = @token), 
                                 (SELECT id FROM live_events WHERE name = @liveEventName),
                                 @guid)
                                 """;

        const string queryWithWorkflowInfo = $"""
                                              WITH inserted_input AS (
                                                {baseQuery}
                                                RETURNING id
                                              )
                                              INSERT INTO derived_workflow_states
                                              (tlg_inputs_id,
                                              resultant_workflow,
                                              in_state)

                                              SELECT id, @workflowId, @workflowState
                                              FROM inserted_input
                                              """;
         
        var commands = tlgInputs.Select(tlgInput =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                ["@tlgUserId"] = tlgInput.TlgAgent.UserId.Id,
                ["@tlgChatId"] = tlgInput.TlgAgent.ChatId.Id,
                ["@lastDataMig"] = 0,
                ["@interactionMode"] = (int)tlgInput.TlgAgent.Mode,
                ["@tlgInputType"] = (int)tlgInput.InputType,
                ["@token"] = tlgInput.OriginatorRole.Match<object>(  
                    r => r.Token, 
                    () => DBNull.Value),  
                ["@liveEventName"] = tlgInput.LiveEventContext.Match<object>(  
                    le => le.Name, 
                    () => DBNull.Value),  
                ["@workflowId"] = tlgInput.ResultantWorkflow.Match<object>(  
                    w => w.WorkflowId, 
                    () => DBNull.Value),  
                ["@workflowState"] = tlgInput.ResultantWorkflow.Match<object>(  
                    w => w.InStateId,  
                    () => DBNull.Value),
                ["@guid"] = tlgInput.EntityGuid.Match<object>(
                    guid => guid,
                    () => DBNull.Value)
            };
            
            var command = GenerateCommand(tlgInput.ResultantWorkflow.IsSome 
                    ? queryWithWorkflowInfo 
                    : baseQuery, 
                normalParameters);

            command.Parameters.Add(new NpgsqlParameter("@tlgMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(tlgInput.Details, Glossary)
            });

            return command;
        }).ToImmutableReadOnlyCollection();

        await ExecuteTransactionAsync(commands);

        foreach (var input in tlgInputs)
        {
            if (_cacheInputsByTlgAgent.TryGetValue(input.TlgAgent, out var cacheForTlgInput))
            {
                cacheForTlgInput.Add(input);
            }

            if (input.LiveEventContext.IsSome)
            {
                if (_cacheInputsByLiveEvent.TryGetValue(input.LiveEventContext.GetValueOrDefault(), 
                        out var cacheForLiveEvent))
                {
                    cacheForLiveEvent.Add(input);
                }
            }
        }
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(TlgAgent tlgAgent) =>
        (await GetAllAsync(tlgAgent))
        .Where(i => i.InputType != TlgInputType.Location)
        .ToImmutableReadOnlyCollection();

    public async Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(ILiveEventInfo liveEvent) =>
        (await GetAllAsync(liveEvent))
        .Where(i => i.InputType != TlgInputType.Location)
        .ToImmutableReadOnlyCollection();

    public async Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(TlgAgent tlgAgent, DateTime since) =>
        (await GetAllAsync(tlgAgent))
        .Where(i => 
            i.InputType == TlgInputType.Location && 
            i.Details.TlgDate >= since)
        .ToImmutableReadOnlyCollection();

    public async Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(ILiveEventInfo liveEvent, DateTime since) =>
        (await GetAllAsync(liveEvent))
        .Where(i => 
            i.InputType == TlgInputType.Location &&
            i.Details.TlgDate >= since)
        .ToImmutableReadOnlyCollection();

    private async Task<IReadOnlyCollection<TlgInput>> GetAllAsync(TlgAgent tlgAgent)
    {
        if (!_cacheInputsByTlgAgent.ContainsKey(tlgAgent))
        {
            await Semaphore.WaitAsync();
        
            try
            {
                if (!_cacheInputsByTlgAgent.ContainsKey(tlgAgent))
                {
                    const string whereClause = """
                                               WHERE inp.user_id = @tlgUserId 
                                               AND inp.chat_id = @tlgChatId 
                                               AND inp.interaction_mode = @mode
                                               """;
                    
                    const string rawQuery = $"{GetAllBaseQuery} {whereClause} {OrderByClause}";
                    
                    var fetchedTlgInputs = new List<TlgInput>(
                        await GetAllExecuteAsync(
                            rawQuery,
                            tlgAgent.UserId, tlgAgent.ChatId, tlgAgent.Mode));

                    _cacheInputsByTlgAgent[tlgAgent] = fetchedTlgInputs;
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cacheInputsByTlgAgent[tlgAgent]
            .ToImmutableReadOnlyCollection();
    }

    private async Task<IReadOnlyCollection<TlgInput>> GetAllAsync(ILiveEventInfo liveEvent)
    {
        if (!_cacheInputsByLiveEvent.ContainsKey(liveEvent))
        {
            await Semaphore.WaitAsync();
        
            try
            {
                if (!_cacheInputsByLiveEvent.ContainsKey(liveEvent))
                {
                    const string whereClause = 
                        "WHERE inp.live_event_id = (SELECT id FROM live_events WHERE name = @liveEventName)";
               
                    const string rawQuery = $"{GetAllBaseQuery} {whereClause} {OrderByClause}";
                    
                    var fetchedTlgInputs = new List<TlgInput>(
                        await GetAllExecuteAsync(
                            rawQuery,
                            liveEventName: liveEvent.Name));

                    _cacheInputsByLiveEvent[liveEvent] = fetchedTlgInputs;
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cacheInputsByLiveEvent[liveEvent]
            .ToImmutableReadOnlyCollection();
    }

    private async Task<IReadOnlyCollection<TlgInput>> GetAllExecuteAsync(
        string rawQuery,
        TlgUserId? userId = null,
        TlgChatId? chatId = null,
        InteractionMode? mode = null,
        string? liveEventName = null)
    {
        var normalParameters = new Dictionary<string, object>();
        
        if (userId != null)
            normalParameters.Add("@tlgUserId", userId.Id);
        if (chatId != null)
            normalParameters.Add("@tlgChatId", chatId.Id);
        if (mode != null)
            normalParameters.Add("@mode", (int) mode);
        if (liveEventName != null)
            normalParameters.Add("@liveEventName", liveEventName);

        var command = GenerateCommand(rawQuery, normalParameters);

        return await ExecuteReaderOneToOneAsync(
            command, ModelReaders.ReadTlgInput);
    }
    
    public async Task HardDeleteAllAsync(TlgAgent tlgAgent)
    {
        const string rawQuery = """
                                WITH inputs_to_delete AS (
                                    SELECT id 
                                    FROM tlg_inputs 
                                    WHERE user_id = @tlgUserId 
                                    AND chat_id = @tlgChatId 
                                    AND interaction_mode = @mode
                                )
                                DELETE FROM derived_workflow_states 
                                WHERE tlg_inputs_id IN (SELECT id FROM inputs_to_delete);

                                DELETE FROM tlg_inputs 
                                WHERE user_id = @tlgUserId 
                                AND chat_id = @tlgChatId 
                                AND interaction_mode = @mode;
                                """;
        
        var normalParameters = new Dictionary<string, object>
        {
            ["@tlgUserId"] = tlgAgent.UserId.Id,
            ["@tlgChatId"] = tlgAgent.ChatId.Id,
            ["@mode"] = (int)tlgAgent.Mode
        };
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new [] { command });
        EmptyCache();
    }

    private void EmptyCache()
    {
        _cacheInputsByTlgAgent = new Dictionary<TlgAgent, List<TlgInput>>();
        _cacheInputsByLiveEvent = new Dictionary<ILiveEventInfo, List<TlgInput>>();
    }
}