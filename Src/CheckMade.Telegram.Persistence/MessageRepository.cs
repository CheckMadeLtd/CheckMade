using CheckMade.Common.Persistence;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Telegram.Persistence;

public class MessageRepository(IDbExecutionHelper dbHelper) : IMessageRepository
{
    public async Task AddAsync(InputMessage inputMessage)
    {
        await dbHelper.ExecuteAsync(async command =>
        {
            command.CommandText = "INSERT INTO tlgr_messages (tlgr_user_id, chat_id, details)" +
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

    public async Task<IEnumerable<InputMessage>> GetAllAsync(long userId)
    {
        var inputMessages = new List<InputMessage>();

        await dbHelper.ExecuteAsync(async command =>
        {
            command.CommandText = "SELECT * FROM tlgr_messages WHERE tlgr_user_id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var telegramUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("tlgr_user_id"));
                    var telegramChatId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("chat_id"));
                    var details = await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details"));

                    var message = new InputMessage(
                        telegramUserId, 
                        telegramChatId,
                        JsonHelper.DeserializeFromJsonStrict<MessageDetails>(details)
                        ?? throw new ArgumentNullException(nameof(details), "Failed to deserialize "));
                    
                    inputMessages.Add(message);
                }
            }
        });

        return inputMessages;
    }
    
    public async Task HardDeleteAsync(long userId)
    {
        await dbHelper.ExecuteAsync(async command =>
        {
            command.CommandText = "DELETE FROM tlgr_messages WHERE tlgr_user_id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            await command.ExecuteNonQueryAsync();
        });
    }}
    