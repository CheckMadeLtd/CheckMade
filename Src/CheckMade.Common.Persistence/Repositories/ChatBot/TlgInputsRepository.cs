using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgInputsRepository(IDbExecutionHelper dbHelper) 
    : BaseRepository(dbHelper), ITlgInputsRepository
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    
    private Option<IReadOnlyCollection<TlgInput>> _cacheInputsByTlgAgent = Option<IReadOnlyCollection<TlgInput>>.None();
    
    public async Task AddAsync(TlgInput tlgInput) =>
        await AddAsync(new List<TlgInput> { tlgInput }.ToImmutableReadOnlyCollection());

    public async Task AddAsync(IReadOnlyCollection<TlgInput> tlgInputs)
    {
        const string rawQuery = "INSERT INTO tlg_inputs " +
                                "(user_id, " +
                                "chat_id, " +
                                "details, " +
                                "last_data_migration, " +
                                "interaction_mode, " +
                                "input_type) " +
                                "VALUES (@tlgUserId, @tlgChatId, @tlgMessageDetails, " +
                                "@lastDataMig, @interactionMode, @tlgInputType)";
        
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
                cache.Concat(tlgInputs).ToImmutableReadOnlyCollection()),
            Option<IReadOnlyCollection<TlgInput>>.None);
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId) =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlg_inputs " +
            "WHERE user_id = @tlgUserId " +
            "ORDER BY id",
            userId, Option<TlgChatId>.None(), Option<InteractionMode>.None());

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgAgent tlgAgent)
    {
        if (_cacheInputsByTlgAgent.IsNone)
        {
            await Semaphore.WaitAsync();

            try
            {
                if (_cacheInputsByTlgAgent.IsNone)
                {
                    const string rawQuery = "SELECT " +
                                            
                                            "r.token AS role_token, " +
                                            "r.role_type AS role_type, " +
                                            "r.status AS role_status, " +
                                            
                                            "lve.name AS live_event_name, " +
                                            "lve.start_date AS live_event_start_date, " +
                                            "lve.end_date AS live_event_end_date, " +
                                            "lve.status AS live_event_status, " +
                                            
                                            "inp.user_id AS input_user_id, " +
                                            "inp.chat_id AS input_chat_id, " +
                                            "inp.interaction_mode AS input_mode, " +
                                            "inp.input_type AS input_type, " +
                                            "inp.details AS input_details " +
                                            
                                            "FROM tlg_inputs inp " +
                                            "LEFT JOIN roles r on inp.role_id = r.id " +
                                            "LEFT JOIN live_events lve on inp.live_event_id = lve.id " +
                                            
                                            "WHERE inp.user_id = @tlgUserId " +
                                            "AND inp.chat_id = @tlgChatId " +
                                            "AND inp.interaction_mode = @mode " +
                                            
                                            "ORDER BY id";
                    
                    var fetchedTlgInputs = new List<TlgInput>(await GetAllExecuteAsync(
                        rawQuery, tlgAgent.UserId, tlgAgent.ChatId, tlgAgent.Mode));

                    _cacheInputsByTlgAgent = Option<IReadOnlyCollection<TlgInput>>.Some(
                        fetchedTlgInputs.ToImmutableReadOnlyCollection());
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        return _cacheInputsByTlgAgent.GetValueOrThrow();
    }

    private async Task<IEnumerable<TlgInput>> GetAllExecuteAsync(
        string rawQuery, Option<TlgUserId> userId, Option<TlgChatId> chatId, Option<InteractionMode> mode)
    {
        var normalParameters = new Dictionary<string, object>();
        
        if (userId.IsSome)
            normalParameters.Add("@tlgUserId", userId.GetValueOrThrow().Id);
        if (chatId.IsSome)
            normalParameters.Add("@tlgChatId", chatId.GetValueOrThrow().Id);
        if (mode.IsSome)
            normalParameters.Add("@mode", (int) mode.GetValueOrThrow());

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

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
        EmptyCash();
    }

    private void EmptyCash() => _cacheInputsByTlgAgent = Option<IReadOnlyCollection<TlgInput>>.None();
}