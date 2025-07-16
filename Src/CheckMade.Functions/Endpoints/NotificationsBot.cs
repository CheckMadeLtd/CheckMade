using System.Collections.Concurrent;
using System.Net;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Bot.Telegram.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CheckMade.Functions.Endpoints;

public sealed class NotificationsBot(IBotFunction botFunction)
{
    // See comment in OperationsBot
    private static readonly ConcurrentDictionary<int, byte> UpdateIds = new();
    private static InteractionMode Mode => InteractionMode.Notifications;

    [Function("NotificationsBot")]
    public HttpResponseData 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        // See comment in OperationsBot
        botFunction.ProcessRequestAsync(request, UpdateIds, Mode);
        
        return request.CreateResponse(HttpStatusCode.OK);
    }
}