using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepo
{
    Task AddAsync(InputTextMessage inputMessage);
    Task<IEnumerable<InputTextMessage>> GetAllAsync(long userId);
}