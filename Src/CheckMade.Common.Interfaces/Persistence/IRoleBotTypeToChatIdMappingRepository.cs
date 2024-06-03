using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence;

public interface IRoleBotTypeToChatIdRepository
{
    Task<IEnumerable<RoleBotTypeToChatIdMapping>> GetAllOrThrowAsync();
}