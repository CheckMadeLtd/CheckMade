using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CheckMade.Telegram.Function.Services.BotClient;

public interface IBotClientFactory
{
    IBotClientWrapper CreateBotClientOrThrow(BotType botType);
}

public class BotClientFactory(
        IHttpClientFactory httpFactory,
        INetworkRetryPolicy retryPolicy,
        BotTokens botTokens,
        ILogger<BotClientWrapper> loggerForClient) 
    : IBotClientFactory
{
    public IBotClientWrapper CreateBotClientOrThrow(BotType botType) => botType switch
    {
        BotType.Operations => new BotClientWrapper(
            new TelegramBotClient(botTokens.OperationsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy, 
            botTokens.OperationsBotToken,
            loggerForClient),
        
        BotType.Communications => new BotClientWrapper(
            new TelegramBotClient(botTokens.CommunicationsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy,
            botTokens.CommunicationsBotToken,
            loggerForClient),
        
        BotType.Notifications => new BotClientWrapper(
            new TelegramBotClient(botTokens.NotificationsBotToken,
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy,
            botTokens.NotificationsBotToken,
            loggerForClient),
        
        _ => throw new ArgumentOutOfRangeException(nameof(botType))
    };
}
