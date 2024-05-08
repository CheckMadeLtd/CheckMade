using System.ComponentModel;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Logic;
using Telegram.Bot;

namespace CheckMade.Telegram.Function.Services;

public interface IBotClientFactory
{
    IBotClientWrapper CreateBotClient(BotType botType);
}

public class BotClientFactory(IHttpClientFactory httpFactory, BotTokens botTokens) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(BotType botType) => botType switch
    {
        BotType.Submissions => new BotClientWrapper(
            new TelegramBotClient(botTokens.SubmissionsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot"))),
        
        BotType.Communications => new BotClientWrapper(
            new TelegramBotClient(botTokens.CommunicationsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot"))),
        
        BotType.Notifications => new BotClientWrapper(
            new TelegramBotClient(botTokens.NotificationsBotToken,
                httpFactory.CreateClient($"CheckMade{botType}Bot"))),
        
        _ => throw new InvalidEnumArgumentException()
    };
}
