using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Services.Persistence.Constitutors;
using CheckMade.Services.Persistence.JsonHelpers;
using General.Utils.FpExtensions.Monads;
using Npgsql;
using NpgsqlTypes;
using static CheckMade.Services.Persistence.Constitutors.StaticConstitutors;

namespace CheckMade.Services.Persistence.Repositories.Bot;

public sealed class InputsRepository(
    IDbExecutionHelper dbHelper, 
    IDomainGlossary glossary,
    InputsConstitutor constitutor) 
    : BaseRepository(dbHelper, glossary), IInputsRepository
{
    public Func<DbDataReader, IDomainGlossary, Input> InputMapper { get; } = 
        (reader, glossary) =>
        {
            var originatorRoleInfo = ConstituteRoleInfo(reader, glossary);
            var liveEventInfo = ConstituteLiveEventInfo(reader);
        
            return constitutor.ConstituteInput(reader, originatorRoleInfo, liveEventInfo, glossary);
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

                                           inp.id AS input_id,
                                           inp.date AS input_date,
                                           inp.message_id AS input_message_id,
                                           inp.user_id AS input_user_id, 
                                           inp.chat_id AS input_chat_id, 
                                           inp.interaction_mode AS input_mode, 
                                           inp.input_type AS input_type,
                                           inp.details AS input_details,
                                           inp.workflow_guid AS input_wfGuid
                                               
                                           FROM inputs inp 
                                           LEFT JOIN roles r on inp.role_id = r.id 
                                           LEFT JOIN live_events le on inp.live_event_id = le.id
                                           LEFT JOIN derived_workflow_states dws on dws.inputs_id = inp.id
                                           """;

    private const string OrderByClause = "ORDER BY inp.id";
    
    private static readonly object CacheLock = new();
    private static readonly Dictionary<Agent, List<Input>> CacheInputsByAgent = new();

    public async Task AddAsync(
        Input input,
        Option<IReadOnlyCollection<ActualSendOutParams>> bridgeDestinations)
    {
        const string addInputQuery = """
                                     INSERT INTO inputs 

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
                                     workflow_guid) 

                                     VALUES (@timeStamp, @messageId, @userId, @chatId, @messageDetails, 
                                     @lastDataMig, @interactionMode, @inputType, 
                                     (SELECT id FROM roles WHERE token = @token), 
                                     (SELECT id FROM live_events WHERE name = @liveEventName),
                                     @guid)
                                     """;

        // The 'inserted_input's below are expected to not be recognised, because they are temp. references defined
        // in the CTE statement below, in the const 'cteAddInputWith', so the IDE can't know about them. 
        
        const string derivedWorkflowInfoQuery = $"""
                                                 INSERT INTO derived_workflow_states
                                                 (inputs_id,
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
            ["@timeStamp"] = input.TimeStamp,
            ["@messageId"] = input.MessageId.Id,
            ["@userId"] = input.Agent.UserId.Id,
            ["@chatId"] = input.Agent.ChatId.Id,
            ["@lastDataMig"] = 0,
            ["@interactionMode"] = (int)input.Agent.Mode,
            ["@inputType"] = (int)input.InputType,
            ["@token"] = input.OriginatorRole.Match<object>(  
                static r => r.Token, 
                static () => DBNull.Value),  
            ["@liveEventName"] = input.LiveEventContext.Match<object>(  
                static le => le.Name, 
                static () => DBNull.Value),  
            ["@workflowId"] = input.ResultantState.Match<object>(  
                static w => w.WorkflowId, 
                static () => DBNull.Value),  
            ["@workflowState"] = input.ResultantState.Match<object>(  
                static w => w.InStateId,  
                static () => DBNull.Value),
            ["@guid"] = input.WorkflowGuid.Match<object>(
                static guid => guid,
                static () => DBNull.Value)
        };

        var command = (input.ResultantState.IsSome, bridgeDestinations.IsSome) switch
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
                $"Saving a {nameof(input)} to DB with {nameof(bridgeDestinations)} but WITHOUT any " +
                $"{nameof(input.ResultantState)} should never be attempted, as it contradicts our fundamental " +
                $"Bot interaction logic.")
        };     
        
        command.Parameters.Add(
            new NpgsqlParameter("@messageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(input.Details, Glossary)
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
                Value = destinations.Select(static d => d.MessageId.Id).ToArray()
            });
        }

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });

        lock (CacheLock)
        {
            if (CacheInputsByAgent.TryGetValue(input.Agent, out var cache))
            {
                cache.Add(input);
            }
        }
    }

    public async Task<IReadOnlyCollection<Input>> GetAllInteractiveAsync(Agent agent) =>
        (await GetAllAsync(agent))
        .Where(static i => i.InputType != InputType.Location)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<Input>> GetAllInteractiveAsync(ILiveEventInfo liveEvent) =>
        (await GetAllAsync(liveEvent))
        .Where(static i => i.InputType != InputType.Location)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<Input>> GetAllLocationAsync(
        Agent agent, DateTimeOffset since) =>
        (await GetAllAsync(agent))
        .Where(i => 
            i.InputType == InputType.Location && 
            i.TimeStamp >= since)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<Input>> GetAllLocationAsync(
        ILiveEventInfo liveEvent, DateTimeOffset since) =>
        (await GetAllAsync(liveEvent))
        .Where(i => 
            i.InputType == InputType.Location &&
            i.TimeStamp >= since)
        .ToImmutableArray();

    public async Task<IReadOnlyCollection<Input>> GetEntityHistoryAsync(ILiveEventInfo liveEvent, Guid entityGuid) =>
        (await GetAllAsync(liveEvent))
        .Where(i =>
            i.WorkflowGuid.GetValueOrDefault() == entityGuid)
        .ToImmutableArray();
    
    public async Task UpdateGuid(IReadOnlyCollection<Input> inputs, Guid newGuid)
    {
        const string rawQuery = """
                                UPDATE inputs 

                                SET workflow_guid = @newGuid

                                WHERE date = @timeStamp
                                AND message_id = @messageId
                                AND user_id = @userId 
                                AND chat_id = @chatId
                                AND interaction_mode = @mode
                                """;

        var commands = inputs.Select(input =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                ["@newGuid"] = newGuid,
                ["@timeStamp"] = input.TimeStamp,
                ["@messageId"] = input.MessageId.Id,
                ["@userId"] = input.Agent.UserId.Id,
                ["@chatId"] = input.Agent.ChatId.Id,
                ["@mode"] = (int)input.Agent.Mode
            };

            return GenerateCommand(rawQuery, normalParameters);
        }).ToArray();
        
        await ExecuteTransactionAsync(commands);

        lock (CacheLock)
        {
            foreach (var grouping in inputs.GroupBy(static i => i.Agent))
            {
                var agent = grouping.Key;
            
                if (CacheInputsByAgent.TryGetValue(agent, out var cache))
                {
                    foreach (var input in grouping)
                    {
                        var index = cache.FindIndex(i =>
                            i.TimeStamp == input.TimeStamp &&
                            i.MessageId == input.MessageId &&
                            i.Agent == input.Agent);

                        /*
                         * No 'else' on purpose: UpdateGuid also applies to the latest/current input BEFORE it has
                         * been added to the database and thus the cache. So we neither want to throw an exception,
                         * nor log a warning, nor add the latest input (since it will be added later via the Add method)
                         */
                        if (index >= 0)
                            cache[index] = input with { WorkflowGuid = newGuid };
                    }
                }        
            }
        }
    }

    private async Task<IReadOnlyCollection<Input>> GetAllAsync(Agent agent)
    {
        lock (CacheLock)
        {
            if (CacheInputsByAgent.TryGetValue(agent, out var cachedInputs))
            {
                return cachedInputs.ToImmutableArray();
            }
        }
        
        const string whereClause = """
                                   WHERE inp.user_id = @userId 
                                   AND inp.chat_id = @chatId 
                                   AND inp.interaction_mode = @mode
                                   """;
                    
        const string rawQuery = $"{GetAllBaseQuery} {whereClause} {OrderByClause}";
                    
        var fetchedInputs = new List<Input>(
            await GetAllExecuteAsync(
                rawQuery,
                agent.UserId, agent.ChatId, agent.Mode));

        
        // Store in cache with lock (double-check pattern)
        lock (CacheLock)
        {
            if (!CacheInputsByAgent.TryGetValue(agent, out var existingCache))
            {
                CacheInputsByAgent[agent] = fetchedInputs;
                
                return fetchedInputs.ToImmutableArray();
            }
        
            // Someone else loaded it while we were querying - use their result
            return existingCache.ToImmutableArray();
        }
    }

    private async Task<IReadOnlyCollection<Input>> GetAllAsync(ILiveEventInfo liveEvent)
    {
        const string whereClause = 
            "WHERE inp.live_event_id = (SELECT id FROM live_events WHERE name = @liveEventName)";
   
        const string rawQuery = $"{GetAllBaseQuery} {whereClause} {OrderByClause}";
        
        return new List<Input>(
            await GetAllExecuteAsync(
                rawQuery,
                liveEventName: liveEvent.Name));
    }

    private async Task<IReadOnlyCollection<Input>> GetAllExecuteAsync(
        string rawQuery,
        UserId? userId = null,
        ChatId? chatId = null,
        InteractionMode? mode = null,
        string? liveEventName = null)
    {
        var normalParameters = new Dictionary<string, object>();
        
        if (userId != null)
            normalParameters.Add("@userId", userId.Id);
        if (chatId != null)
            normalParameters.Add("@chatId", chatId.Id);
        if (mode != null)
            normalParameters.Add("@mode", (int)mode);
        if (liveEventName != null)
            normalParameters.Add("@liveEventName", liveEventName);

        var command = GenerateCommand(rawQuery, normalParameters);

        return await ExecuteMapperAsync(command, InputMapper);
    }
    
    public async Task HardDeleteAllAsync(Agent agent)
    {
        const string rawQuery = """
                                WITH inputs_to_delete AS (
                                    SELECT id 
                                    FROM inputs 
                                    WHERE user_id = @userId 
                                    AND chat_id = @chatId 
                                    AND interaction_mode = @mode
                                )
                                DELETE FROM derived_workflow_states 
                                WHERE inputs_id IN (SELECT id FROM inputs_to_delete);

                                DELETE FROM inputs 
                                WHERE user_id = @userId 
                                AND chat_id = @chatId 
                                AND interaction_mode = @mode;
                                """;
        
        var normalParameters = new Dictionary<string, object>
        {
            ["@userId"] = agent.UserId.Id,
            ["@chatId"] = agent.ChatId.Id,
            ["@mode"] = (int)agent.Mode
        };
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync([command]);

        lock (CacheLock)
        {
            CacheInputsByAgent.Remove(agent);
        }
    }
}