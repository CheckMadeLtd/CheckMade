using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Persistence;
using CheckMade.Common.Persistence.JsonHelpers;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.DTOs;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Telegram.Persistence;

public class MessageRepository(IDbExecutionHelper dbHelper) : IMessageRepository
{
    public async Task AddOrThrowAsync(InputMessageDto inputMessage)
    {
        await AddOrThrowAsync(new List<InputMessageDto> { inputMessage }.ToImmutableArray());
    }

    public async Task AddOrThrowAsync(IEnumerable<InputMessageDto> inputMessages)
    {
        var commands = inputMessages.Select(inputMessage =>
        {
            var command = new NpgsqlCommand("INSERT INTO tlgr_updates " +
                                            "(user_id, chat_id, details, last_data_migration, bot_type, update_type)" +
                                            " VALUES (@telegramUserId, @telegramChatId, @telegramMessageDetails," +
                                            "@lastDataMig, @botType, @updateType)");

            command.Parameters.AddWithValue("@telegramUserId", inputMessage.UserId);
            command.Parameters.AddWithValue("@telegramChatId", inputMessage.ChatId);
            command.Parameters.AddWithValue("@lastDataMig", 0);
            command.Parameters.AddWithValue("@botType", (int) inputMessage.BotType);
            command.Parameters.AddWithValue("@updateType", (int) inputMessage.ModelUpdateType);

            command.Parameters.Add(new NpgsqlParameter("@telegramMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(inputMessage.Details)
            });

            return command;
        }).ToImmutableArray();

        await dbHelper.ExecuteOrThrowAsync(async (db, transaction) =>
        {
            foreach (var command in commands)
            {
                command.Connection = db;
                command.Transaction = transaction;        
                await command.ExecuteNonQueryAsync();
            }
        });
    }

    public async Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync() =>
        await GetAllOrThrowExecuteAsync(
            "SELECT * FROM tlgr_updates",
            Option<UserId>.None());

    public async Task<IEnumerable<InputMessageDto>> GetAllOrThrowAsync(UserId userId) =>
        await GetAllOrThrowExecuteAsync(
            "SELECT * FROM tlgr_updates WHERE user_id = @userId",
            userId);

    private async Task<IEnumerable<InputMessageDto>> GetAllOrThrowExecuteAsync(string commandText, Option<UserId> userId)
    {
        var builder = ImmutableArray.CreateBuilder<InputMessageDto>();
        var command = new NpgsqlCommand(commandText);
            
        if (userId.IsSome)
            command.Parameters.AddWithValue("@userId", userId.GetValueOrDefault());

        await dbHelper.ExecuteOrThrowAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    builder.Add(await CreateInputMessageFromReaderStrictAsync(reader));
                }
            }
        });

        return builder.ToImmutable();
    }
    
    private static async Task<InputMessageDto> CreateInputMessageFromReaderStrictAsync(DbDataReader reader)
    {
        var telegramUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        var telegramChatId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("chat_id"));
        var telegramBotType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("bot_type"));
        var telegramUpdateType = await reader.GetFieldValueAsync<int>(reader.GetOrdinal("update_type"));
        var details = await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details"));

        var message = new InputMessageDto(
            telegramUserId,
            telegramChatId,
            (BotType) telegramBotType,
            (ModelUpdateType) telegramUpdateType,
            JsonHelper.DeserializeFromJsonStrict<InputMessageDetails>(details) 
            ?? throw new InvalidOperationException("Failed to deserialize"));

        return message;
    }

    public async Task HardDeleteAllOrThrowAsync(UserId userId)
    {
        var command = new NpgsqlCommand("DELETE FROM tlgr_updates WHERE user_id = @userId");
        command.Parameters.AddWithValue("@userId", userId);

        await dbHelper.ExecuteOrThrowAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        });
    }
}