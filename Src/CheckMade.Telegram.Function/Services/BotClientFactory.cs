using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Logic;
using Telegram.Bot;

namespace CheckMade.Telegram.Function.Services;

public interface IBotClientFactory
{
    IBotClientWrapper CreateBotClientOrThrow(BotType botType);
}

public class BotClientFactory(
        IHttpClientFactory httpFactory,
        INetworkRetryPolicy retryPolicy,
        BotTokens botTokens) 
    : IBotClientFactory
{
    public IBotClientWrapper CreateBotClientOrThrow(BotType botType) => botType switch
    {
        BotType.Submissions => new BotClientWrapper(
            new TelegramBotClient(botTokens.SubmissionsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy,
            botTokens.SubmissionsBotToken),
        
        BotType.Communications => new BotClientWrapper(
            new TelegramBotClient(botTokens.CommunicationsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy,
            botTokens.CommunicationsBotToken),
        
        BotType.Notifications => new BotClientWrapper(
            new TelegramBotClient(botTokens.NotificationsBotToken,
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy,
            botTokens.NotificationsBotToken),
        
        _ => throw new ArgumentOutOfRangeException()
    };
}
