using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.Tlg;

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
            var command = new NpgsqlCommand("INSERT INTO tlgr_updates " +
                                            "(user_id, chat_id, details, last_data_migration, bot_type, update_type)" +
                                            " VALUES (@tlgUserId, @tlgChatId, @tlgMessageDetails," +
                                            "@lastDataMig, @tlgBotType, @tlgInputType)");

            command.Parameters.AddWithValue("@tlgUserId", (long) tlgInput.UserId);
            command.Parameters.AddWithValue("@tlgChatId", (long) tlgInput.ChatId);
            command.Parameters.AddWithValue("@lastDataMig", 0);
            command.Parameters.AddWithValue("@tlgBotType", (int) tlgInput.BotType);
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
            "SELECT * FROM tlgr_updates",
            Option<TlgUserId>.None());

    public async Task<IEnumerable<TlgInput>> GetAllAsync(TlgUserId userId) =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlgr_updates WHERE user_id = @tlgUserId",
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
                    builder.Add(await CreateTlgInputFromReaderStrictAsync(reader));
                }
            }
        });

        return builder.ToImmutable();
    }
    
    private static async Task<TlgInput> CreateTlgInputFromReaderStrictAsync(DbDataReader reader)
    {
        TlgUserId tlgUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        TlgChatId tlgChatId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("chat_id"));
        var tlgBotType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("bot_type"));
        var tlgInputType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("update_type"));
        var tlgDetails = await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details"));

        var message = new TlgInput(
            tlgUserId,
            tlgChatId,
            (TlgBotType) tlgBotType,
            (TlgInputType) tlgInputType,
            JsonHelper.DeserializeFromJsonStrict<TlgInputDetails>(tlgDetails) 
            ?? throw new InvalidOperationException("Failed to deserialize"));

        return message;
    }

    public async Task HardDeleteAllAsync(TlgUserId userId)
    {
        var command = new NpgsqlCommand("DELETE FROM tlgr_updates WHERE user_id = @tlgUserId");
        command.Parameters.AddWithValue("@tlgUserId", (long) userId);

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        });
    }
}