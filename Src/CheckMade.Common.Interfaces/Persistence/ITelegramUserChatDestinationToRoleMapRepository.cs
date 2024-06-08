using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence;

public interface ITelegramUserChatDestinationToRoleMapRepository
{
    Task<IEnumerable<TelegramUserChatDestinationToRoleMap>> GetAllAsync();
}