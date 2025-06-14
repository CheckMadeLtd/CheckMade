using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;
using static CheckMade.Common.Persistence.Repositories.DomainModelConstitutors;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public sealed class TlgInputsRepository(IDbExecutionHelper dbHelper, IDomainGlossary glossary) 
    : BaseRepository(dbHelper, glossary), ITlgInputsRepository
{
    internal static readonly Func<DbDataReader, IDomainGlossary, TlgInput> TlgInputMapper = 
        static (reader, glossary) =>
        {
            var originatorRoleInfo = ConstituteRoleInfo(reader, glossary);
            var liveEventInfo = ConstituteLiveEventInfo(reader);
        
            return ConstituteTlgInput(reader, originatorRoleInfo, liveEventInfo, glossary);
        };
    
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

                                           inp.date AS input_date,
                                           inp.message_id AS input_message_id,
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

    public async Task AddAsync(
        TlgInput tlgInput,
        Option<IReadOnlyCollection<ActualSendOutParams>> bridgeDestinations)
    {
        const string addInputQuery = """
                                     INSERT INTO tlg_inputs 

                                     (date,
                                     message_id,
                                     user_id, 
                                     chat_id, 
                                     details, 
                                     last_data_migration, 
                                     interaction_mode, 
                                     input_type, 
                                     role_id, 
                                     live_event_id,
                                     entity_guid) 

                                     VALUES (@tlgDate, @tlgMessageId, @tlgUserId, @tlgChatId, @tlgMessageDetails, 
                                     @lastDataMig, @interactionMode, @tlgInputType, 
                                     (SELECT id FROM roles WHERE token = @token), 
                                     (SELECT id FROM live_events WHERE name = @liveEventName),
                                     @guid)
                                     """;

        const string derivedWorkflowInfoQuery = $"""
                                                 INSERT INTO derived_workflow_states
                                                 (tlg_inputs_id,
                                                 resultant_workflow,
                                                 in_state)

                                                 SELECT id, @workflowId, @workflowState
                                                 FROM inserted_input
                                                 """;

        const string derivedWorkflowBridgesInfoQuery = $"""
                                                        INSERT INTO derived_workflow_bridges
                                                        (src_input_id,
                                                         dst_chat_id,
                                                         dst_message_id)
                                                         
                                                        SELECT id, unnest(@dstChatIds), unnest(@dstMessageIds)
                                                        FROM inserted_input
                                                        """;

        const string cteAddInputWith = $"""
                                        WITH inserted_input AS (
                                            {addInputQuery}
                                            RETURNING id
                                        )
                                        """;

        const string cteWorkflowAs = $"""
                                      derived_workflow AS (
                                          {derivedWorkflowInfoQuery}
                                      )
                                      """;
        
        var normalParameters = new Dictionary<string, object>
        {
            ["@tlgDate"] = tlgInput.TlgDate,
            ["@tlgMessageId"] = tlgInput.TlgMessageId.Id,
            ["@tlgUserId"] = tlgInput.TlgAgent.UserId.Id,
            ["@tlgChatId"] = tlgInput.TlgAgent.ChatId.Id,
            ["@lastDataMig"] = 0,
            ["@interactionMode"] = (int)tlgInput.TlgAgent.Mode,
            ["@tlgInputType"] = (int)tlgInput.InputType,
            ["@token"] = tlgInput.OriginatorRole.Match<object>(  
                static r => r.Token, 
                static () => DBNull.Value),  
            ["@liveEventName"] = tlgInput.LiveEventContext.Match<object>(  
                static le => le.Name, 
                static () => DBNull.Value),  
            ["@workflowId"] = tlgInput.ResultantState.Match<object>(  
                static w => w.WorkflowId, 
                static () => DBNull.Value),  
            ["@workflowState"] = tlgInput.ResultantState.Match<object>(  
                static w => w.InStateId,  
                static () => DBNull.Value),
            ["@guid"] = tlgInput.EntityGuid.Match<object>(
                static guid => guid,
                static () => DBNull.Value)
        };

        var command = (tlgInput.ResultantState.IsSome, bridgeDestinations.IsSome) switch
        {
            (false, false) => 
                GenerateCommand(addInputQuery, 
                    normalParameters),
            
            (true, false) => 
                GenerateCommand($"{cteAddInputWith}\n{derivedWorkflowInfoQuery};",
                    normalParameters),
            
            (true, true) =>
                GenerateCommand($"{cteAddInputWith},\n{cteWorkflowAs}\n{derivedWorkflowBridgesInfoQuery};", 
                    normalParameters),
            
            _ => throw new InvalidOperationException(
                $"Saving a {nameof(tlgInput)} to DB with {nameof(bridgeDestinations)} but WITHOUT any " +
                $"{nameof(tlgInput.ResultantState)} should never be attempted, as it contradicts our fundamental " +
                $"ChatBot interaction logic.")
        };     
        
        command.Parameters.Add(
            new NpgsqlParameter("@tlgMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(tlgInput.Details, Glossary)
            });

        // Suppressing warning due to Npgsql library design:
        // NpgsqlDbType supports bitwise operations for array types, but is not marked as [Flags]
        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        if (bridgeDestinations.IsSome)
        {
            var destinations = bridgeDestinations.GetValueOrThrow();

            command.Parameters.Add(new NpgsqlParameter("@dstChatIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
            {
                Value = destinations.Select(static d => d.ChatId.Id).ToArray()
            });
            
            command.Parameters.Add(new NpgsqlParameter("@dstMessageIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
            {
                Value = destinations.Select(static d => d.TlgMessageId.Id).ToArray()
            });
        }

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
        
        if (_cacheInputsByTlgAgent.TryGetValue(tlgInput.TlgAgent, out var cacheForTlgInput))
        {
            cacheForTlgInput.Add(tlgInput);
        }

        if (tlgInput.LiveEventContext.IsSome)
        {
            if (_cacheInputsByLiveEvent.TryGetValue(tlgInput.LiveEventContext.GetValueOrDefault(), 
                    out var cacheForLiveEvent))
            {
                cacheForLiveEvent.Add(tlgInput);
            }
        }
    }

    public async Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(TlgAgent tlgAgent) =>
        (await GetAllAsync(tlgAgent))
        .Where(static i => i.InputType != TlgInputType.Location)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<TlgInput>> GetAllInteractiveAsync(ILiveEventInfo liveEvent) =>
        (await GetAllAsync(liveEvent))
        .Where(static i => i.InputType != TlgInputType.Location)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(
        TlgAgent tlgAgent, DateTimeOffset since) =>
        (await GetAllAsync(tlgAgent))
        .Where(i => 
            i.InputType == TlgInputType.Location && 
            i.TlgDate >= since)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<TlgInput>> GetAllLocationAsync(
        ILiveEventInfo liveEvent, DateTimeOffset since) =>
        (await GetAllAsync(liveEvent))
        .Where(i => 
            i.InputType == TlgInputType.Location &&
            i.TlgDate >= since)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<TlgInput>> GetEntityHistoryAsync(ILiveEventInfo liveEvent, Guid entityGuid) =>
        (await GetAllAsync(liveEvent))
        .Where(i =>
            i.EntityGuid.GetValueOrDefault() == entityGuid)
        .ToImmutableArray();
    
    public async Task UpdateGuid(IReadOnlyCollection<TlgInput> tlgInputs, Guid newGuid)
    {
        const string rawQuery = """
                                UPDATE tlg_inputs 

                                SET entity_guid = @newGuid

                                WHERE date = @tlgDate
                                AND message_id = @tlgMessageId
                                AND user_id = @userId 
                                AND chat_id = @chatId
                                AND interaction_mode = @mode
                                """;

        var commands = tlgInputs.Select(tlgInput =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                ["@newGuid"] = newGuid,
                ["@tlgDate"] = tlgInput.TlgDate,
                ["@tlgMessageId"] = tlgInput.TlgMessageId.Id,
                ["@userId"] = tlgInput.TlgAgent.UserId.Id,
                ["@chatId"] = tlgInput.TlgAgent.ChatId.Id,
                ["@mode"] = (int)tlgInput.TlgAgent.Mode
            };

            return GenerateCommand(rawQuery, normalParameters);
        }).ToArray();
        
        await ExecuteTransactionAsync(commands);
        EmptyCache();
    }

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

        return _cacheInputsByTlgAgent[tlgAgent];
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

        return _cacheInputsByLiveEvent[liveEvent];
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
            normalParameters.Add("@mode", (int)mode);
        if (liveEventName != null)
            normalParameters.Add("@liveEventName", liveEventName);

        var command = GenerateCommand(rawQuery, normalParameters);

        return await ExecuteMapperAsync(
            command, TlgInputMapper);
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

        await ExecuteTransactionAsync([command]);
        EmptyCache();
    }

    private void EmptyCache()
    {
        _cacheInputsByTlgAgent = new Dictionary<TlgAgent, List<TlgInput>>();
        _cacheInputsByLiveEvent = new Dictionary<ILiveEventInfo, List<TlgInput>>();
    }
}