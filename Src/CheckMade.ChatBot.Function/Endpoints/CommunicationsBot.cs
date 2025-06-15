using System.Collections.Concurrent;
using CheckMade.ChatBot.Telegram.Function;
using CheckMade.Common.DomainModel.Data.ChatBot.UserInteraction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CheckMade.ChatBot.Function.Endpoints;

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
