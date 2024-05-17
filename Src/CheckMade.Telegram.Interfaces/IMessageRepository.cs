using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepository
{
    Task AddOrThrowAsync(InputMessage inputMessage);
    Task<IEnumerable<InputMessage>> GetAllOrThrowAsync();
    Task<IEnumerable<InputMessage>> GetAllOrThrowAsync(long userId);
    Task HardDeleteOrThrowAsync(long userId);
}