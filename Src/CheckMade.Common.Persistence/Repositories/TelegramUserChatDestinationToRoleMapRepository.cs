using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Persistence.Repositories;

public class TelegramUserChatDestinationToRoleMapRepository : ITelegramUserChatDestinationToRoleMapRepository
{
    public Task<IEnumerable<TelegramUserChatDestinationToRoleMap>> GetAllAsync()
    {
        var builder = ImmutableList.CreateBuilder<TelegramUserChatDestinationToRoleMap>();
        
        return Task.FromResult<IEnumerable<TelegramUserChatDestinationToRoleMap>>(builder.ToImmutable());
    }
}