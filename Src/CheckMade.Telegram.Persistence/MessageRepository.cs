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
        await dbHelper.ExecuteOrThrowAsync(async command =>
        {
            command.CommandText = "INSERT INTO tlgr_messages (user_id, chat_id, details)" +
                                  " VALUES (@telegramUserId, @telegramChatId, @telegramMessageDetails)";
            command.Parameters.AddWithValue("@telegramUserId", inputMessage.UserId);
            command.Parameters.AddWithValue("@telegramChatId", inputMessage.ChatId);
            
            command.Parameters.Add(new NpgsqlParameter("@telegramMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(inputMessage.Details)
            });

            await command.ExecuteNonQueryAsync();
        });
    }

    public Task<IEnumerable<InputMessage>> GetAllOrThrowAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<InputMessage>> GetAllOrThrowAsync(long userId)
    {
        var inputMessages = new List<InputMessage>();

        await dbHelper.ExecuteOrThrowAsync(async command =>
        {
            command.CommandText = "SELECT * FROM tlgr_messages WHERE user_id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    inputMessages.Add(await CreateInputMessageFromReaderAsync(reader));
                }
            }
        });

        return inputMessages;
    }
    
    public async Task HardDeleteOrThrowAsync(long userId)
    {
        await dbHelper.ExecuteOrThrowAsync(async command =>
        {
            command.CommandText = "DELETE FROM tlgr_messages WHERE user_id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            await command.ExecuteNonQueryAsync();
        });
    }

    private static async Task<InputMessage> CreateInputMessageFromReaderAsync(DbDataReader reader)
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
}