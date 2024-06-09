using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Persistence.JsonHelpers;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence.Repositories;

public class TelegramUpdateRepository(IDbExecutionHelper dbHelper) : ITelegramUpdateRepository
{
    public async Task AddAsync(TelegramUpdate telegramUpdate)
    {
        await AddAsync(new List<TelegramUpdate> { telegramUpdate }.ToImmutableArray());
    }

    public async Task AddAsync(IEnumerable<TelegramUpdate> telegramUpdates)
    {
        var commands = telegramUpdates.Select(update =>
        {
            var command = new NpgsqlCommand("INSERT INTO tlgr_updates " +
                                            "(user_id, chat_id, details, last_data_migration, bot_type, update_type)" +
                                            " VALUES (@telegramUserId, @telegramChatId, @telegramMessageDetails," +
                                            "@lastDataMig, @botType, @updateType)");

            command.Parameters.AddWithValue("@telegramUserId", (long) update.UserId);
            command.Parameters.AddWithValue("@telegramChatId", (long) update.ChatId);
            command.Parameters.AddWithValue("@lastDataMig", 0);
            command.Parameters.AddWithValue("@botType", (int) update.BotType);
            command.Parameters.AddWithValue("@updateType", (int) update.ModelUpdateType);

            command.Parameters.Add(new NpgsqlParameter("@telegramMessageDetails", NpgsqlDbType.Jsonb)
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

    public async Task<IEnumerable<TelegramUpdate>> GetAllAsync() =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlgr_updates",
            Option<TelegramUserId>.None());

    public async Task<IEnumerable<TelegramUpdate>> GetAllAsync(TelegramUserId userId) =>
        await GetAllExecuteAsync(
            "SELECT * FROM tlgr_updates WHERE user_id = @userId",
            userId);

    private async Task<IEnumerable<TelegramUpdate>> GetAllExecuteAsync(
        string commandText, Option<TelegramUserId> userId)
    {
        var builder = ImmutableArray.CreateBuilder<TelegramUpdate>();
        var command = new NpgsqlCommand(commandText);
            
        if (userId.IsSome)
            command.Parameters.AddWithValue("@userId", (long) userId.GetValueOrThrow());

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    builder.Add(await CreateTelegramUpdateFromReaderStrictAsync(reader));
                }
            }
        });

        return builder.ToImmutable();
    }
    
    private static async Task<TelegramUpdate> CreateTelegramUpdateFromReaderStrictAsync(DbDataReader reader)
    {
        TelegramUserId telegramUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        TelegramChatId telegramChatId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("chat_id"));
        var telegramBotType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("bot_type"));
        var telegramUpdateType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("update_type"));
        var details = await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details"));

        var message = new TelegramUpdate(
            telegramUserId,
            telegramChatId,
            (BotType) telegramBotType,
            (ModelUpdateType) telegramUpdateType,
            JsonHelper.DeserializeFromJsonStrict<TelegramUpdateDetails>(details) 
            ?? throw new InvalidOperationException("Failed to deserialize"));

        return message;
    }

    public async Task HardDeleteAllAsync(TelegramUserId userId)
    {
        var command = new NpgsqlCommand("DELETE FROM tlgr_updates WHERE user_id = @userId");
        command.Parameters.AddWithValue("@userId", (long) userId);

        await dbHelper.ExecuteAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        });
    }
}