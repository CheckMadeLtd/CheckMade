using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Function.Endpoints;

public sealed class CommunicationsBot(ILogger<CommunicationsBot> logger, IBotUpdateSwitch botUpdateSwitch)
    : BotFunctionBase(logger, botUpdateSwitch)
{
    protected override InteractionMode InteractionMode => InteractionMode.Communications;

    [Function("CommunicationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}
