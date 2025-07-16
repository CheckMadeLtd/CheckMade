using System.Collections.Concurrent;
using System.Net;
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
    public HttpResponseData 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        // See comment in OperationsBot
        botFunction.ProcessRequestAsync(request, UpdateIds, Mode);
        
        return request.CreateResponse(HttpStatusCode.OK);
    }
}
