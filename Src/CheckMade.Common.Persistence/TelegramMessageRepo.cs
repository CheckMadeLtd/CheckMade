using CheckMade.Common.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace CheckMade.Common.Persistence;

public class TelegramMessageRepo(IDbConnectionProvider dbProvider) 
    : ITelegramMessageRepo
{
    public void Add(long telegramUserId, string telegramMessageText)
    {
        using (var db = dbProvider.CreateConnection())
        {
            db.Open();
            
            var sql = new NpgsqlCommand(
                "INSERT INTO tlgr_messages (tlgr_user_id, details)" +
                " VALUES (@telegramUserId, @telegramMessageText)", (NpgsqlConnection)db);
            
            sql.Parameters.AddWithValue("@telegramUserId", telegramUserId);
            
            sql.Parameters.Add(new NpgsqlParameter("@telegramMessageText", NpgsqlDbType.Jsonb)
            {
                Value = JsonHelper.SerializeToJson(new { text = telegramMessageText })
            });
            
            sql.ExecuteNonQuery();
        }
    }
    
    
}