using CheckMade.Chat.Logic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.Chat.Telegram.Functions;

public class NotificationsBot(ILogger<NotificationsBot> logger, UpdateService updateService)
    : TelegramBotFunctionBase(logger, updateService)
{
    protected override BotType BotType => BotType.Notifications;

    [Function("NotificationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}