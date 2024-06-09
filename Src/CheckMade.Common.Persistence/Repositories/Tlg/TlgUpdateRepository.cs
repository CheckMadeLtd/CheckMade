using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Tlg.Updates;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories.Tlg;

public class TlgUpdateRepository(IDbExecutionHelper dbHelper) : ITlgUpdateRepository
{
    public async Task AddAsync(TlgUpdate tlgUpdate)
    {
        await AddAsync(new List<TlgUpdate> { tlgUpdate }.ToImmutableArray());
    }

    public async Task AddAsync(IEnumerable<TlgUpdate> tlgUpdates)
    {
        var commands = tlgUpdates.Select(update =>
        {
            var command = new NpgsqlCommand("INSERT INTO tlgr_updates " +
                                            "(user_id, chat_id, details, last_data_migration, bot_type, update_type)" +
                                            " VALUES (@tlgUserId, @tlgChatId, @tlgMessageDetails," +
                                            "@lastDataMig, @tlgBotType, @tlgUpdateType)");

            command.Parameters.AddWithValue("@tlgUserId", (long) update.UserId);
            command.Parameters.AddWithValue("@tlgChatId", (long) update.ChatId);
            command.Parameters.AddWithValue("@lastDataMig", 0);
            command.Parameters.AddWithValue("@tlgBotType", (int) update.BotType);
            command.Parameters.AddWithValue("@tlgUpdateType", (int) update.TlgUpdateType);

            command.Parameters.Add(new NpgsqlParameter("@tlgMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(update.Details)
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

    public async Task<IEnumerable<TlgUpdate>> GetAllAsync() =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlgr_updates",
            Option<TlgUserId>.None());

    public async Task<IEnumerable<TlgUpdate>> GetAllAsync(TlgUserId userId) =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlgr_updates WHERE user_id = @tlgUserId",
            userId);

    private async Task<IEnumerable<TlgUpdate>> GetAllExecuteAsync(
        string commandText, Option<TlgUserId> userId)
    {
        var builder = ImmutableArray.CreateBuilder<TlgUpdate>();
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
                    builder.Add(await CreateTlgUpdateFromReaderStrictAsync(reader));
                }
            }
        });

        return builder.ToImmutable();
    }
    
    private static async Task<TlgUpdate> CreateTlgUpdateFromReaderStrictAsync(DbDataReader reader)
    {
        TlgUserId tlgUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        TlgChatId tlgChatId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("chat_id"));
        var tlgBotType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("bot_type"));
        var tlgUpdateType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("update_type"));
        var tlgDetails = await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details"));

        var message = new TlgUpdate(
            tlgUserId,
            tlgChatId,
            (TlgBotType) tlgBotType,
            (TlgUpdateType) tlgUpdateType,
            JsonHelper.DeserializeFromJsonStrict<TlgUpdateDetails>(tlgDetails) 
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