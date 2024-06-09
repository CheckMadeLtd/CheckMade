using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Persistence.Repositories;

public class TelegramPortToRoleMapRepository : ITelegramPortToRoleMapRepository
{
    public Task<IEnumerable<TelegramPortToRoleMap>> GetAllAsync()
    {
        var builder = ImmutableList.CreateBuilder<TelegramPortToRoleMap>();
        
        return Task.FromResult<IEnumerable<TelegramPortToRoleMap>>(builder.ToImmutable());
    }
}