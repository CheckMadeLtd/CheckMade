using CheckMade.Common.Persistence;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepository
{
    Task AddOrThrowAsync(InputMessage inputMessage);
    Task AddOrThrowAsync(IEnumerable<InputMessage> inputMessages);
    Task<IEnumerable<InputMessage>> GetAllOrThrowAsync();
    Task<IEnumerable<InputMessage>> GetAllOrThrowAsync(long userId);
    Task MigrateOrThrowAsync(IEnumerable<UpdateDetails> updateDetails);
    Task HardDeleteAllOrThrowAsync(long userId);
}