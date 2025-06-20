using System.Collections.Concurrent;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Bot.Telegram.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CheckMade.Functions.Endpoints;

public sealed class CommunicationsBot(IBotFunction botFunction)
{
    // See comment in OperationsBot
    private static readonly ConcurrentDictionary<int, byte> UpdateIds = new();
    private static InteractionMode Mode => InteractionMode.Communications;

    [Function("CommunicationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await botFunction.ProcessRequestAsync(request, UpdateIds, Mode);
    }
}
