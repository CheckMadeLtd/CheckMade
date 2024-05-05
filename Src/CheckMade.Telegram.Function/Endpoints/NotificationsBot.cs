using CheckMade.Telegram.Logic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.Telegram.Function.Endpoints;

public class NotificationsBot(ILogger<NotificationsBot> logger, UpdateService updateService)
    : BotFunctionBase(logger, updateService)
{
    protected override BotType BotType => BotType.Notifications;

    [Function("NotificationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}