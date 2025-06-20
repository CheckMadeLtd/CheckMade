using CheckMade.Core.Model.Bot.Categories;
using General.Utils.RetryPolicies;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CheckMade.Bot.Telegram.BotClient;

public interface IBotClientFactory
{
    IBotClientWrapper CreateBotClient(InteractionMode interactionMode);
}

public sealed class BotClientFactory(
    IHttpClientFactory httpFactory,
    INetworkRetryPolicy retryPolicy,
    BotTokens botTokens,
    ILogger<BotClientWrapper> loggerForClient) 
    : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(InteractionMode interactionMode) => interactionMode switch
    {
        InteractionMode.Operations => new BotClientWrapper(
            new TelegramBotClient(botTokens.OperationsBotToken, 
                httpFactory.CreateClient($"CheckMade{interactionMode}Bot")),
            retryPolicy, 
            interactionMode,
            botTokens.OperationsBotToken,
            loggerForClient),
        
        InteractionMode.Communications => new BotClientWrapper(
            new TelegramBotClient(botTokens.CommunicationsBotToken, 
                httpFactory.CreateClient($"CheckMade{interactionMode}Bot")),
            retryPolicy,
            interactionMode,
            botTokens.CommunicationsBotToken,
            loggerForClient),
        
        InteractionMode.Notifications => new BotClientWrapper(
            new TelegramBotClient(botTokens.NotificationsBotToken,
                httpFactory.CreateClient($"CheckMade{interactionMode}Bot")),
            retryPolicy,
            interactionMode,
            botTokens.NotificationsBotToken,
            loggerForClient),
        
        _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
    };
}
