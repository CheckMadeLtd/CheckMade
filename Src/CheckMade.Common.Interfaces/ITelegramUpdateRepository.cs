using CheckMade.Common.Model.TelegramUpdates;

namespace CheckMade.Common.Interfaces;

public interface ITelegramUpdateRepository
{
    Task AddOrThrowAsync(TelegramUpdate telegramUpdate);
    Task AddOrThrowAsync(IEnumerable<TelegramUpdate> telegramUpdates);
    Task<IEnumerable<TelegramUpdate>> GetAllOrThrowAsync();
    Task<IEnumerable<TelegramUpdate>> GetAllOrThrowAsync(TelegramUserId userId);
    Task HardDeleteAllOrThrowAsync(TelegramUserId userId);
}