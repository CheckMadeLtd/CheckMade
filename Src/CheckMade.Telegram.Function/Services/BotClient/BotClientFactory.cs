using CheckMade.Common.Model.Tlg;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Function.Startup;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CheckMade.Telegram.Function.Services.BotClient;

public interface IBotClientFactory
{
    IBotClientWrapper CreateBotClient(TlgInteractionMode interactionMode);
}

public class BotClientFactory(
        IHttpClientFactory httpFactory,
        INetworkRetryPolicy retryPolicy,
        BotTokens botTokens,
        ILogger<BotClientWrapper> loggerForClient) 
    : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(TlgInteractionMode interactionMode) => interactionMode switch
    {
        TlgInteractionMode.Operations => new BotClientWrapper(
            new TelegramBotClient(botTokens.OperationsBotToken, 
                httpFactory.CreateClient($"CheckMade{interactionMode}Bot")),
            retryPolicy, 
            interactionMode,
            botTokens.OperationsBotToken,
            loggerForClient),
        
        TlgInteractionMode.Communications => new BotClientWrapper(
            new TelegramBotClient(botTokens.CommunicationsBotToken, 
                httpFactory.CreateClient($"CheckMade{interactionMode}Bot")),
            retryPolicy,
            interactionMode,
            botTokens.CommunicationsBotToken,
            loggerForClient),
        
        TlgInteractionMode.Notifications => new BotClientWrapper(
            new TelegramBotClient(botTokens.NotificationsBotToken,
                httpFactory.CreateClient($"CheckMade{interactionMode}Bot")),
            retryPolicy,
            interactionMode,
            botTokens.NotificationsBotToken,
            loggerForClient),
        
        _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
    };
}
