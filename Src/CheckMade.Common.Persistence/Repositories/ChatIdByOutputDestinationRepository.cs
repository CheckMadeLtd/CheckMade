using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Persistence.Repositories;

public class ChatIdByOutputDestinationRepository : IChatIdByOutputDestinationRepository
{
    // The combination of Role & BotType needs to be unique i.e. each RoleBotType can only have one ChatId 
    
    public Task<IEnumerable<RoleBotTypeToChatIdMapping>> GetAllOrThrowAsync()
    {
        var builder = ImmutableList.CreateBuilder<RoleBotTypeToChatIdMapping>();
        
        return Task.FromResult<IEnumerable<RoleBotTypeToChatIdMapping>>(builder.ToImmutable());
    }
}