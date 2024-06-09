using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence;

public interface ITelegramPortToRoleMapRepository
{
    Task<IEnumerable<TelegramPortToRoleMap>> GetAllAsync();
}