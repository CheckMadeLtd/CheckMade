using CheckMade.Common.Interfaces;
using CheckMade.Common.Persistence;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Telegram.Persistence;

public class MessageRepo(IDbConnectionProvider dbProvider) : IMessageRepo
{
    public void Add(InputTextMessage inputMessage)
    {
        using (var db = dbProvider.CreateConnection())
        {
            db.Open();
            
            var sql = new NpgsqlCommand(
                "INSERT INTO tlgr_messages (tlgr_user_id, details)" +
                " VALUES (@telegramUserId, @telegramMessageText)", (NpgsqlConnection)db);
            
            sql.Parameters.AddWithValue("@telegramUserId", inputMessage.UserId);
            
            sql.Parameters.Add(new NpgsqlParameter("@telegramMessageText", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(inputMessage.Details)
            });
            
            sql.ExecuteNonQuery();
        }
    }

    public IEnumerable<InputTextMessage> GetAll(long userId)
    {
        using (var db = dbProvider.CreateConnection())
        {
            db.Open();

            var sql = new NpgsqlCommand("SELECT * FROM tlgr_messages WHERE tlgr_user_id = @userId",
                (NpgsqlConnection)db);

            sql.Parameters.AddWithValue("@userId", userId);

            var reader = sql.ExecuteReader();

            var inputMessages = new List<InputTextMessage>();

            while (reader.Read())
            {
                var telegramUserId = reader.GetInt64(reader.GetOrdinal("tlgr_user_id"));
                var details = reader.GetString(reader.GetOrdinal("details"));

                var message = new InputTextMessage(
                    telegramUserId,
                    JsonHelper.DeserializeFromJson<MessageDetails>(details)
                        ?? throw new ArgumentNullException(nameof(details)));

                inputMessages.Add(message);
            };

            return inputMessages;
        }
    }
}