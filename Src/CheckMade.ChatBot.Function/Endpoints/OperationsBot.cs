using System.Collections.Concurrent;
using CheckMade.ChatBot.Telegram.Function;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CheckMade.ChatBot.Function.Endpoints;

public sealed class OperationsBot(IBotFunction botFunction)
{
    // Thread-safe collection for this static cache (ensuring atomic operations in case of multiple function instances)
    private static readonly ConcurrentDictionary<int, byte> UpdateIds = new();
    private static InteractionMode Mode => InteractionMode.Operations;

    [Function("OperationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await botFunction.ProcessRequestAsync(request, UpdateIds, Mode);
    }
}