using System.Collections.Concurrent;
using CheckMade.Abstract.Domain.Model.Bot.Categories;
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
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await botFunction.ProcessRequestAsync(request, UpdateIds, Mode);
    }
}