using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.ChatBot;

public class TlgInputRepository(IDbExecutionHelper dbHelper) : ITlgInputRepository
{
    public async Task AddAsync(TlgInput tlgInput)
    {
        await AddAsync(new List<TlgInput> { tlgInput }.ToImmutableArray());
    }

    public async Task AddAsync(IEnumerable<TlgInput> tlgInputs)
    {
        var commands = tlgInputs.Select(tlgInput =>
        {
            var command = new NpgsqlCommand("INSERT INTO tlg_inputs " +
                                            "(user_id, chat_id, details, last_data_migration, " +
                                            "interaction_mode, input_type)" +
                                            " VALUES (@tlgUserId, @tlgChatId, @tlgMessageDetails," +
                                            "@lastDataMig, @interactionMode, @tlgInputType)");

            command.Parameters.AddWithValue("@tlgUserId", (long) tlgInput.UserId);
            command.Parameters.AddWithValue("@tlgChatId", (long) tlgInput.ChatId);
            command.Parameters.AddWithValue("@lastDataMig", 0);
            command.Parameters.AddWithValue("@interactionMode", (int) tlgInput.InteractionMode);
            command.Parameters.AddWithValue("@tlgInputType", (int) tlgInput.TlgInputType);

            command.Parameters.Add(new NpgsqlParameter("@tlgMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(tlgInput.Details)
            });

            return command;
        }).ToImmutableArray();

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            foreach (var command in commands)
            {
                command.Connection = db;
                command.Transaction = transaction;        
                await command.ExecuteNonQueryAsync();
            }
        });
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
        string commandText, Option<TlgUserId> userId)
    {
        var builder = ImmutableArray.CreateBuilder<TlgInput>();
        var command = new NpgsqlCommand(commandText);
            
        if (userId.IsSome)
            command.Parameters.AddWithValue("@tlgUserId", (long) userId.GetValueOrThrow());

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    builder.Add(CreateTlgInputFromReaderStrict(reader));
                }
            }
        });

        return builder.ToImmutable();
    }
    
    private static TlgInput CreateTlgInputFromReaderStrict(DbDataReader reader)
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
    }

    public async Task HardDeleteAllAsync(TlgUserId userId)
    {
        var command = new NpgsqlCommand("DELETE FROM tlg_inputs WHERE user_id = @tlgUserId");
        command.Parameters.AddWithValue("@tlgUserId", (long) userId);

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        });
    }
}