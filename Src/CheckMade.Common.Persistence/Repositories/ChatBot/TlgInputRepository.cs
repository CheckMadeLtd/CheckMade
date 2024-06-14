using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgInputRepository(IDbExecutionHelper dbHelper) : BaseRepository(dbHelper), ITlgInputRepository
{
    public async Task AddAsync(TlgInput tlgInput) =>
        await AddAsync(new List<TlgInput> { tlgInput }.ToImmutableArray());

    public async Task AddAsync(IEnumerable<TlgInput> tlgInputs)
    {
        const string rawQuery = "INSERT INTO tlg_inputs " +
                                "(user_id, chat_id, details, last_data_migration, " +
                                "interaction_mode, input_type)" +
                                " VALUES (@tlgUserId, @tlgChatId, @tlgMessageDetails," +
                                "@lastDataMig, @interactionMode, @tlgInputType)";
        
        var commands = tlgInputs.Select(tlgInput =>
        {
            var normalParameters = new Dictionary<string, object>
            {
                { "@tlgUserId", (long)tlgInput.UserId },
                { "@tlgChatId", (long)tlgInput.ChatId },
                { "@lastDataMig", 0 },
                { "@interactionMode", (int)tlgInput.InteractionMode },
                { "@tlgInputType", (int) tlgInput.TlgInputType }
            };
            
            var command = GenerateCommand(rawQuery, normalParameters);
            
            command.Parameters.Add(new NpgsqlParameter("@tlgMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(tlgInput.Details)
            });
            
            return command;
        }).ToImmutableArray();

        await ExecuteTransactionAsync(commands);
    }

    public async Task<IEnumerable<TlgInput>> GetAllAsync() =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlg_inputs",
            Option<TlgUserId>.None());

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId) =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlg_inputs WHERE user_id = @tlgUserId",
            userId);

    private async Task<IEnumerable<TlgInput>> GetAllExecuteAsync(
        string rawQuery, Option<TlgUserId> userId)
    {
        var normalParameters = new Dictionary<string, object>();
        
        if (userId.IsSome)
            normalParameters.Add("@tlgUserId", (long) userId.GetValueOrThrow());

        var command = GenerateCommand(rawQuery, normalParameters);

        return await ExecuteReaderAsync(command, reader =>
        {
            TlgUserId tlgUserId = reader.GetInt64(reader.GetOrdinal("user_id"));
            TlgChatId tlgChatId = reader.GetInt64(reader.GetOrdinal("chat_id"));
            var interactionMode = reader.GetInt16(reader.GetOrdinal("interaction_mode"));
            var tlgInputType = reader.GetInt16(reader.GetOrdinal("input_type"));
            var tlgDetails = reader.GetString(reader.GetOrdinal("details"));

            var message = new TlgInput(
                tlgUserId,
                tlgChatId,
                (InteractionMode) interactionMode,
                (TlgInputType) tlgInputType,
                JsonHelper.DeserializeFromJsonStrict<TlgInputDetails>(tlgDetails) 
                ?? throw new InvalidOperationException("Failed to deserialize"));

            return message;
        });
    }
    
    public async Task HardDeleteAllAsync(TlgUserId userId)
    {
        const string rawQuery = "DELETE FROM tlg_inputs WHERE user_id = @tlgUserId";
        var normalParameters = new Dictionary<string, object> { { "@tlgUserId", (long)userId } };
        var command = GenerateCommand(rawQuery, normalParameters);

        await ExecuteTransactionAsync(new List<NpgsqlCommand> { command });
    }
}