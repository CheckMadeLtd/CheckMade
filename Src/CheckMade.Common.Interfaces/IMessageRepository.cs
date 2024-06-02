using CheckMade.Common.Model.TelegramUpdates;

namespace CheckMade.Common.Interfaces;

public interface IMessageRepository
{
    Task AddOrThrowAsync(TelegramUpdate telegramUpdate);
    Task AddOrThrowAsync(IEnumerable<TelegramUpdate> inputMessages);
    Task<IEnumerable<TelegramUpdate>> GetAllOrThrowAsync();
    Task<IEnumerable<TelegramUpdate>> GetAllOrThrowAsync(TelegramUserId userId);
    Task HardDeleteAllOrThrowAsync(TelegramUserId userId);
}