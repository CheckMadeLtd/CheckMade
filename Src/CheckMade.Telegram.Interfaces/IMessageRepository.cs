using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepository
{
    Task AddAsync(InputTextMessage inputMessage);
    Task<IEnumerable<InputTextMessage>> GetAllAsync(long userId);
    Task HardDeleteAsync(long userId);
}