using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Persistence.Repositories;

public class ChatIdByOutputDestinationRepository : IChatIdByOutputDestinationRepository
{
    // The combination of Role & BotType needs to be unique i.e. each RoleBotType can only have one ChatId 
    
    public Task<IEnumerable<ChatIdByOutputDestination>> GetAllOrThrowAsync()
    {
        var builder = ImmutableList.CreateBuilder<ChatIdByOutputDestination>();
        
        return Task.FromResult<IEnumerable<ChatIdByOutputDestination>>(builder.ToImmutable());
    }
}