using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Function.Startup;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CheckMade.Telegram.Function.Services.BotClient;

public interface IBotClientFactory
{
    IBotClientWrapper CreateBotClient(BotType botType);
}

public class BotClientFactory(
        IHttpClientFactory httpFactory,
        INetworkRetryPolicy retryPolicy,
        BotTokens botTokens,
        ILogger<BotClientWrapper> loggerForClient) 
    : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(BotType botType) => botType switch
    {
        BotType.Operations => new BotClientWrapper(
            new TelegramBotClient(botTokens.OperationsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy, 
            botType,
            botTokens.OperationsBotToken,
            loggerForClient),
        
        BotType.Communications => new BotClientWrapper(
            new TelegramBotClient(botTokens.CommunicationsBotToken, 
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy,
            botType,
            botTokens.CommunicationsBotToken,
            loggerForClient),
        
        BotType.Notifications => new BotClientWrapper(
            new TelegramBotClient(botTokens.NotificationsBotToken,
                httpFactory.CreateClient($"CheckMade{botType}Bot")),
            retryPolicy,
            botType,
            botTokens.NotificationsBotToken,
            loggerForClient),
        
        _ => throw new ArgumentOutOfRangeException(nameof(botType))
    };
}
