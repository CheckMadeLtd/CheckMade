using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence;

public interface IChatIdByOutputDestinationRepository
{
    Task<IEnumerable<ChatIdByOutputDestination>> GetAllAsync();
}