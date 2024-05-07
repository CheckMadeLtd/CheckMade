using System.ComponentModel;
using CheckMade.Telegram.Logic;
using Telegram.Bot;

namespace CheckMade.Telegram.Function.Services;

public interface IBotClientFactory
{
    ITelegramBotClientAdapter CreateBotClient(BotType botType);
}

public class BotClientFactory(IHttpClientFactory httpFactory, BotTokens botTokens) : IBotClientFactory
{
    public ITelegramBotClientAdapter CreateBotClient(BotType botType) => botType switch
    {
        BotType.Submissions => new TelegramBotClientAdapter(
            new TelegramBotClient(botTokens.SubmissionsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot"))),
        
        BotType.Communications => new TelegramBotClientAdapter(
            new TelegramBotClient(botTokens.CommunicationsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot"))),
        
        BotType.Notifications => new TelegramBotClientAdapter(
            new TelegramBotClient(botTokens.NotificationsBotToken,
                httpFactory.CreateClient($"CheckMade{botType}Bot"))),
        
        _ => throw new InvalidEnumArgumentException()
    };
}
