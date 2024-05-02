using System.ComponentModel;
using CheckMade.Chat.Logic;
using Telegram.Bot;

namespace CheckMade.Chat.Telegram.Startup;

public interface ITelegramBotClientFactory
{
    ITelegramBotClient CreateBotClient(BotType botType);
}

public class TelegramBotClientFactory(IHttpClientFactory httpFactory) : ITelegramBotClientFactory
{
    public ITelegramBotClient CreateBotClient(BotType botType) => botType switch
    {
        BotType.Submissions => new TelegramBotClient("token",
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        BotType.Communications => new TelegramBotClient("token",
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        BotType.Notifications => new TelegramBotClient("token",
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        _ => throw new InvalidEnumArgumentException()
    };
}
