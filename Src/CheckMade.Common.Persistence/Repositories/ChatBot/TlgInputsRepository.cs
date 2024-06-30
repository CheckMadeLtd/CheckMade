using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgInputsRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ITlgInputsRepository
{
    private const string GetAllBaseQuery =   """
                                             SELECT
                                                 
                                             r.token AS role_token, 
                                             r.role_type AS role_type, 
                                             r.status AS role_status, 
                                                 
                                             lve.name AS live_event_name, 
                                             lve.start_date AS live_event_start_date, 
                                             lve.end_date AS live_event_end_date, 
                                             lve.status AS live_event_status, 
                                                 
                                             inp.user_id AS input_user_id, 
                                             inp.chat_id AS input_chat_id, 
                                             inp.interaction_mode AS input_mode, 
                                             inp.input_type AS input_type, 
                                             inp.details AS input_details 
                                                 
                                             FROM tlg_inputs inp 
                                             LEFT JOIN roles r on inp.role_id = r.id 
                                             LEFT JOIN live_events lve on inp.live_event_id = lve.id
                                             """;

    private const string OrderByClause = "ORDER BY inp.id";
    
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    // ToDo: Remove this warning after key-based cache implementation
    // WARNING:
    /*
     * Current caching strategy assumes each GetAllAsync is not called with a different parameter in the same scope.
     * This assumption may need to be revisited when implementing cross-event queries (see also pitfalls documentation).
     */
    
    private Option<IReadOnlyCollection<TlgInput>> _cacheInputsByTlgAgent = 
        Option<IReadOnlyCollection<TlgInput>>.None();
    
    private Option<IReadOnlyCollection<TlgInput>> _cacheInputsByLiveEvent = 
        Option<IReadOnlyCollection<TlgInput>>.None();
    
    public async Task AddAsync(TlgInput tlgInput) =>
        await AddAsync(new [] { tlgInput }.ToImmutableReadOnlyCollection());

    public async Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs)
    {
        const string rawQuery = "INSERT INTO tlg_inputs " +
                                "(user_id, " +
                                "chat_id, " +
                                "details, " +
                                "last_data_migration, " +
                                "interaction_mode, " +
                                "input_type, " +
                                "role_id, " +
                                "live_event_id) " +
                                "VALUES (@tlgUserId, @tlgChatId, @tlgMessageDetails, " +
                                "@lastDataMig, @interactionMode, @tlgInputType, " +
                                "(SELECT id FROM roles WHERE token = @token), " +
                                "(SELECT id FROM live_events WHERE name = @liveEventName))";
        
        var commands = tlgInputs.Select(tlgInput =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                { "@tlgUserId", tlgInput.TlgAgent.UserId.Id },
                { "@tlgChatId", tlgInput.TlgAgent.ChatId.Id },
                { "@lastDataMig", 0 },
                { "@interactionMode", (int)tlgInput.TlgAgent.Mode },
                { "@tlgInputType", (int) tlgInput.InputType }
            };
            
            if (tlgInput.OriginatorRole.IsSome)
                normalParameters.Add("@token", tlgInput.OriginatorRole.GetValueOrThrow().Token);
            else
                normalParameters.Add("@token", DBNull.Value);    
            
            if (tlgInput.LiveEventContext.IsSome)
                normalParameters.Add("@liveEventName", tlgInput.LiveEventContext.GetValueOrThrow().Name);
            else
                normalParameters.Add("@liveEventName", DBNull.Value);    
            
            var command = GenerateCommand(rawQuery, normalParameters);
            
            command.Parameters.Add(new NpgsqlParameter("@tlgMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(tlgInput.Details)
            });

            return command;
        }).ToImmutableReadOnlyCollection();

        await ExecuteTransactionAsync(commands);

        _cacheInputsByTlgAgent = _cacheInputsByTlgAgent.Match(
            cache => Option<IReadOnlyCollection<TlgInput>>.Some(
                cache.Concat(tlgInputs)
                    .ToImmutableReadOnlyCollection()),
            Option<IReadOnlyCollection<TlgInput>>.None);
        
        // ToDo: only add to this cache if the input has a ILiveEventInfo ?! 
        _cacheInputsByLiveEvent = _cacheInputsByLiveEvent.Match(
            cache => Option<IReadOnlyCollection<TlgInput>>.Some(
                cache.Concat(tlgInputs)
                    .ToImmutableReadOnlyCollection()),
            Option<IReadOnlyCollection<TlgInput>>.None);
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgAgent tlgAgent)
    {
        if (_cacheInputsByTlgAgent.IsNone)
        {
            await Semaphore.WaitAsync();
        
            try
            {
                if (_cacheInputsByTlgAgent.IsNone)
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

                    _cacheInputsByTlgAgent = Option<IReadOnlyCollection<TlgInput>>.Some(
                        fetchedTlgInputs
                            .ToImmutableReadOnlyCollection());
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cacheInputsByTlgAgent.GetValueOrThrow();
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync(ILiveEventInfo liveEvent)
    {
        if (_cacheInputsByLiveEvent.IsNone)
        {
            await Semaphore.WaitAsync();
        
            try
            {
                if (_cacheInputsByLiveEvent.IsNone)
                {
                    const string whereClause = 
                        "WHERE inp.live_event_id = (SELECT id FROM live_events WHERE name = @liveEventName)";
               
                    const string rawQuery = $"{GetAllBaseQuery} {whereClause} {OrderByClause}";
                    
                    var fetchedTlgInputs = new List<TlgInput>(
                        await GetAllExecuteAsync(
                            rawQuery,
                            liveEventName: liveEvent.Name));

                    _cacheInputsByLiveEvent = Option<IReadOnlyCollection<TlgInput>>.Some(
                        fetchedTlgInputs
                            .ToImmutableReadOnlyCollection());
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cacheInputsByLiveEvent.GetValueOrThrow();
    }

    private async Task<IEnumerable<TlgInput>> GetAllExecuteAsync(
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

        return await ExecuteReaderAsync(command, ReadTlgInput);
    }
    
    public async Task HardDeleteAllAsync(TlgAgent tlgAgent)
    {
        const string rawQuery = "DELETE FROM tlg_inputs " +
                                "WHERE user_id = @tlgUserId " +
                                "AND chat_id = @tlgChatId " +
                                "AND interaction_mode = @mode";
        
        var normalParameters = new Dictionary<string, object>
        {
            { "@tlgUserId", tlgAgent.UserId.Id },
            { "@tlgChatId", tlgAgent.ChatId.Id },
            { "@mode", (int) tlgAgent.Mode }
        };
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new [] { command });
        EmptyCash();
    }

    private void EmptyCash()
    {
        _cacheInputsByTlgAgent = Option<IReadOnlyCollection<TlgInput>>.None();
        _cacheInputsByLiveEvent = Option<IReadOnlyCollection<TlgInput>>.None();
    }
}