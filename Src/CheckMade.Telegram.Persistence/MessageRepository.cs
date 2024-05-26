using System.Collections.Immutable;
using System.Data.Common;
using CheckMade.Common.Persistence;
using CheckMade.Common.Persistence.JsonHelpers;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Telegram.Persistence;

public class MessageRepository(IDbExecutionHelper dbHelper) : IMessageRepository
{
    public async Task AddOrThrowAsync(InputMessage inputMessage)
    {
        await AddOrThrowAsync(new List<InputMessage> { inputMessage }.ToImmutableArray());
    }

    public async Task AddOrThrowAsync(IEnumerable<InputMessage> inputMessages)
    {
        var commands = inputMessages.Select(inputMessage =>
        {
            var command = new NpgsqlCommand("INSERT INTO tlgr_messages (user_id, chat_id, details)" +
                                            " VALUES (@telegramUserId, @telegramChatId, @telegramMessageDetails)");

            command.Parameters.AddWithValue("@telegramUserId", inputMessage.UserId);
            command.Parameters.AddWithValue("@telegramChatId", inputMessage.ChatId);

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

    public async Task<IEnumerable<InputMessage>> GetAllOrThrowAsync() =>
        await GetAllOrThrowExecuteAsync(
            "SELECT * FROM tlgr_messages",
            Option<long>.None());

    public async Task<IEnumerable<InputMessage>> GetAllOrThrowAsync(long userId) =>
        await GetAllOrThrowExecuteAsync(
            "SELECT * FROM tlgr_messages WHERE user_id = @userId",
            userId);

    private async Task<IEnumerable<InputMessage>> GetAllOrThrowExecuteAsync(string commandText, Option<long> userId)
    {
        var builder = ImmutableArray.CreateBuilder<InputMessage>();
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
    
    private static async Task<InputMessage> CreateInputMessageFromReaderStrictAsync(DbDataReader reader)
    {
        var telegramUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("user_id"));
        var telegramChatId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("chat_id"));
        var details = await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details"));

        var message = new InputMessage(
            telegramUserId,
            telegramChatId,
            JsonHelper.DeserializeFromJsonStrict<MessageDetails>(details) 
            ?? throw new InvalidOperationException("Failed to deserialize"));

        return message;
    }

    public async Task HardDeleteAllOrThrowAsync(long userId)
    {
        var command = new NpgsqlCommand("DELETE FROM tlgr_messages WHERE user_id = @userId");
        command.Parameters.AddWithValue("@userId", userId);

        await dbHelper.ExecuteOrThrowAsync(async (db, transaction) =>
        {
            command.Connection = db;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        });
    }
}