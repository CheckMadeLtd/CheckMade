using System.ComponentModel;
using CheckMade.Telegram.Logic;
using Telegram.Bot;

namespace CheckMade.Telegram.Function.Services;

public interface IBotClientFactory
{
    ITelegramBotClient CreateBotClient(BotType botType);
}

public class BotClientFactory(IHttpClientFactory httpFactory, BotTokens botTokens) 
    : IBotClientFactory
{
    public ITelegramBotClient CreateBotClient(BotType botType) => botType switch
    {
        BotType.Submissions => new TelegramBotClient(botTokens.SubmissionsBotToken,
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        BotType.Communications => new TelegramBotClient(botTokens.CommunicationsBotToken,
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        BotType.Notifications => new TelegramBotClient(botTokens.NotificationsBotToken,
            httpFactory.CreateClient($"CheckMade{botType}Bot")),
        
        _ => throw new InvalidEnumArgumentException()
    };
}
