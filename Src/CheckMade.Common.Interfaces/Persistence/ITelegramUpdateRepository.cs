using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Common.Interfaces.Persistence;

public interface ITelegramUpdateRepository
{
    Task AddOrThrowAsync(TelegramUpdate telegramUpdate);
    Task AddOrThrowAsync(IEnumerable<TelegramUpdate> telegramUpdates);
    Task<IEnumerable<TelegramUpdate>> GetAllOrThrowAsync();
    Task<IEnumerable<TelegramUpdate>> GetAllOrThrowAsync(TelegramUserId userId);
    Task HardDeleteAllOrThrowAsync(TelegramUserId userId);
}