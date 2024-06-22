using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Persistence.JsonHelpers;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgInputsRepository(IDbExecutionHelper dbHelper, ILogger<BaseRepository> logger) 
    : BaseRepository(dbHelper, logger), ITlgInputsRepository
{
    public async Task AddAsync(TlgInput tlgInput) =>
        await AddAsync(new List<TlgInput> { tlgInput }.ToImmutableReadOnlyCollection());

    public async Task AddAsync(IEnumerable<TlgInput> tlgInputs)
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
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId) =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlg_inputs " +
            "WHERE user_id = @tlgUserId " +
            "ORDER BY id",
            userId, Option<TlgChatId>.None(), Option<InteractionMode>.None());

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgAgent tlgAgent) =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlg_inputs " +
            "WHERE user_id = @tlgUserId " +
            "AND chat_id = @tlgChatId " +
            "AND interaction_mode = @mode " +
            "ORDER BY id",
            tlgAgent.UserId, tlgAgent.ChatId, tlgAgent.Mode);

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

        return await ExecuteReaderAsync(command, reader =>
        {
            TlgUserId tlgUserId = reader.GetInt64(reader.GetOrdinal("user_id"));
            TlgChatId tlgChatId = reader.GetInt64(reader.GetOrdinal("chat_id"));
            var interactionMode = EnsureEnumValidityOrThrow(
                (InteractionMode)reader.GetInt16(reader.GetOrdinal("interaction_mode")));
            var tlgInputType = EnsureEnumValidityOrThrow(
                (TlgInputType)reader.GetInt16(reader.GetOrdinal("input_type")));
            var tlgDetails = reader.GetString(reader.GetOrdinal("details"));

            var message = new TlgInput(
                new TlgAgent(tlgUserId, tlgChatId, interactionMode),
                tlgInputType,
                JsonHelper.DeserializeFromJsonStrict<TlgInputDetails>(tlgDetails) 
                ?? throw new InvalidOperationException("Failed to deserialize"));

            return message;
        });
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
    }
}