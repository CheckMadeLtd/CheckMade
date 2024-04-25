using CheckMade.Common.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence;

public class TelegramMessageRepo(IDbConnectionProvider dbProvider) 
    : ITelegramMessageRepo
{
    public void Add(long telegramUserId, string messageText)
    {
        using (var db = dbProvider.CreateConnection())
        {
            db.Open();
            
            var sql = new NpgsqlCommand(
                "INSERT INTO telegram_messages (telegram_user_id, details)" +
                " VALUES (@telegramUserId, @MessageText)", (NpgsqlConnection)db);
            
            sql.Parameters.AddWithValue("@telegramUserId", telegramUserId);
            
            sql.Parameters.Add(new NpgsqlParameter("@MessageText", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(new { text = messageText })
            });
            
            sql.ExecuteNonQuery();
        }
    }
}