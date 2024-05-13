using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepository
{
    Task AddAsync(InputMessage inputMessage);
    Task<IEnumerable<InputMessage>> GetAllAsync();
    Task<IEnumerable<InputMessage>> GetAllAsync(long userId);
    Task HardDeleteAsync(long userId);
}