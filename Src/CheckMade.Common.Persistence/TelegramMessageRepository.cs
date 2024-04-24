using CheckMade.Common.Interfaces;
using Npgsql;

namespace CheckMade.Common.Persistence;

public record TelegramMessageRepository
{
    private readonly IDbConnectionProvider _dbProvider;

    public TelegramMessageRepository(IDbConnectionProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public void Add(long telegramUserId, string messageText)
    {
        using (var db = _dbProvider.CreateConnection())
        {
            db.Open();
            
            var sql = new NpgsqlCommand(
                "INSERT INTO telegram_messages (telegram_user_id, details)" +
                " VALUES (@telegramUserId, @MessageText)");
            
            sql.Parameters.AddWithValue("@telegramUserId", telegramUserId);
            sql.Parameters.AddWithValue("@MessageText", messageText);
            
            sql.ExecuteNonQuery();
        }
    }
}