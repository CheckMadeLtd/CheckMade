using CheckMade.Common.Interfaces;
using CheckMade.Common.Persistence;
using CheckMade.Telegram.Interfaces;
using Npgsql;
using NpgsqlTypes;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Persistence;

public class MessageRepo(IDbConnectionProvider dbProvider) : IMessageRepo
{
    public void Add(Message message)
    {
        using (var db = dbProvider.CreateConnection())
        {
            db.Open();
            
            var sql = new NpgsqlCommand(
                "INSERT INTO tlgr_messages (tlgr_user_id, details)" +
                " VALUES (@telegramUserId, @telegramMessageText)", (NpgsqlConnection)db);
            
            sql.Parameters.AddWithValue("@telegramUserId", message.From.Id);
            
            sql.Parameters.Add(new NpgsqlParameter("@telegramMessageText", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(new MessageDetails(message.Text))
            });
            
            sql.ExecuteNonQuery();
        }
    }

    public IEnumerable<Message> GetAll(long userId)
    {
        using (var db = dbProvider.CreateConnection())
        {
            db.Open();

            var sql = new NpgsqlCommand("SELECT * FROM tlgr_messages WHERE tlgr_user_id = @userId",
                (NpgsqlConnection)db);

            sql.Parameters.AddWithValue("@userId", userId);

            var reader = sql.ExecuteReader();

            var messages = new List<Message>();

            while (reader.Read())
            {
                var telegramUserId = reader.GetInt64(reader.GetOrdinal("tlgr_user_id"));
                var details = reader.GetString(reader.GetOrdinal("details"));

                var message = new Message
                {
                    From = new User { Id = telegramUserId }, 
                    Text = JsonHelper.DeserializeFromJson<MessageDetails>(details).Text
                };

                messages.Add(message);
            }

            return messages;
        }
    }

    private record MessageDetails(string? Text);
}