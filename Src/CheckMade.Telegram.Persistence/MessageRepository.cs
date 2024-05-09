using CheckMade.Common.Persistence;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Telegram.Persistence;

// ToDo: Add try/catch / exception handling for:  db Conneciton not good; deserialisation failed; wrong columns for reader.

public class MessageRepository(IDbExecutionHelper dbHelper) : IMessageRepository
{
    public async Task AddAsync(InputTextMessage inputMessage)
    {
        await dbHelper.ExecuteAsync(async command =>
        {
            command.CommandText = "INSERT INTO tlgr_messages (tlgr_user_id, details)" +
                                  " VALUES (@telegramUserId, @telegramMessageDetails)";
            command.Parameters.AddWithValue("@telegramUserId", inputMessage.UserId);
            
            command.Parameters.Add(new NpgsqlParameter("@telegramMessageDetails", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(inputMessage.Details)
            });

            await command.ExecuteNonQueryAsync();
        });
    }

    public async Task<IEnumerable<InputTextMessage>> GetAllAsync(long userId)
    {
        var inputMessages = new List<InputTextMessage>();
    
        await dbHelper.ExecuteAsync(async command =>
        {
            command.CommandText = "SELECT * FROM tlgr_messages WHERE tlgr_user_id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var telegramUserId = await reader.GetFieldValueAsync<long>(reader.GetOrdinal("tlgr_user_id"));
                    var details = await reader.GetFieldValueAsync<string>(reader.GetOrdinal("details"));

                    var message = new InputTextMessage(
                        telegramUserId, 
                        JsonHelper.DeserializeFromJson<MessageDetails>(details)
                        ?? throw new ArgumentNullException(nameof(details)));

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
    