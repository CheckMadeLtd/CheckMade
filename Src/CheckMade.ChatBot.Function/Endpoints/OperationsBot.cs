using System.Collections.Concurrent;
using CheckMade.ChatBot.Telegram.UpdateHandling;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Function.Endpoints;

public sealed class OperationsBot(ILogger<OperationsBot> logger, IBotUpdateSwitch botUpdateSwitch)
    : BotFunctionBase(logger, botUpdateSwitch)
{
    private static readonly ConcurrentDictionary<int, byte> UpdateIds = new();

    protected override ConcurrentDictionary<int, byte> CurrentlyProcessingUpdateIds => UpdateIds;
    protected override InteractionMode InteractionMode => InteractionMode.Operations;

    [Function("OperationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}