using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Persistence.Repositories;

public class RoleBotTypeToChatIdMappingRepository : IRoleBotTypeToChatIdRepository
{
    public Task<IEnumerable<RoleBotTypeToChatIdMapping>> GetAllOrThrowAsync()
    {
        var builder = ImmutableList.CreateBuilder<RoleBotTypeToChatIdMapping>();
        
        return Task.FromResult<IEnumerable<RoleBotTypeToChatIdMapping>>(builder.ToImmutable());
    }
}