using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories;

internal class MockRoleBotTypeToChatIdRepository : IRoleBotTypeToChatIdRepository
{
    public Task<IEnumerable<RoleBotTypeToChatIdMapping>> GetAllOrThrowAsync()
    {
        var builder = ImmutableArray.CreateBuilder<RoleBotTypeToChatIdMapping>();
        
        return Task.FromResult<IEnumerable<RoleBotTypeToChatIdMapping>>(builder.ToImmutable());
    }
}