using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Function.Endpoints;

public class NotificationsBot(ILogger<NotificationsBot> logger, IBotUpdateSwitch botUpdateSwitch)
    : BotFunctionBase(logger, botUpdateSwitch)
{
    protected override InteractionMode InteractionMode => InteractionMode.Notifications;

    [Function("NotificationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}