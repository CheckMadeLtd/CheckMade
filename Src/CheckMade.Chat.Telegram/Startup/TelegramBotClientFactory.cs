using System.ComponentModel;
using CheckMade.Chat.Logic;
using CheckMade.Common.Utilities;
using Telegram.Bot;

namespace CheckMade.Chat.Telegram.Startup;

public interface ITelegramBotClientFactory
{
    ITelegramBotClient CreateBotClient(BotType botType);
}

public class TelegramBotClientFactory(IHttpClientFactory httpFactory, AppSettings appSettings) 
    : ITelegramBotClientFactory
{
    public ITelegramBotClient CreateBotClient(BotType botType) => botType switch
    {
        BotType.Submissions => new TelegramBotClient(appSettings.TelegramSubmissionsBotToken,
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        BotType.Communications => new TelegramBotClient(appSettings.TelegramCommunicationsBotToken,
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        BotType.Notifications => new TelegramBotClient(appSettings.TelegramNotificationsBotToken,
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        _ => throw new InvalidEnumArgumentException()
    };
}
