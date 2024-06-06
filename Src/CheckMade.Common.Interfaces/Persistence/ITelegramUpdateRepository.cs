using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Common.Interfaces.Persistence;

public interface ITelegramUpdateRepository
{
    Task AddAsync(TelegramUpdate telegramUpdate);
    Task AddAsync(IEnumerable<TelegramUpdate> telegramUpdates);
    Task<IEnumerable<TelegramUpdate>> GetAllAsync();
    Task<IEnumerable<TelegramUpdate>> GetAllAsync(TelegramUserId userId);
    Task HardDeleteAllAsync(TelegramUserId userId);
}