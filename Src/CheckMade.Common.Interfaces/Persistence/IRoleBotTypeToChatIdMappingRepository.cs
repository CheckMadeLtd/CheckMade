using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Interfaces.Persistence;

public interface IRoleBotTypeToChatIdMappingRepository
{
    Task<IEnumerable<RoleBotTypeToChatIdMapping>> GetAllOrThrowAsync();
}