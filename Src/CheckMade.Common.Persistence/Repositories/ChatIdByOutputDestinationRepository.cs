using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Persistence.Repositories;

public class ChatIdByOutputDestinationRepository : IChatIdByOutputDestinationRepository
{
    // The OutputDestination (i.e. the the combinations of Role and BotType) need to be unique
    // i.e. each OutputDestination can only have one ChatId!
    
    public Task<IEnumerable<ChatIdByOutputDestination>> GetAllAsync()
    {
        var builder = ImmutableList.CreateBuilder<ChatIdByOutputDestination>();
        
        return Task.FromResult<IEnumerable<ChatIdByOutputDestination>>(builder.ToImmutable());
    }
}