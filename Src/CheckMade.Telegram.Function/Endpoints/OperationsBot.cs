using CheckMade.Common.Model.Core.Enums;
using CheckMade.Common.Model.Tlg;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.Telegram.Function.Endpoints;

public class OperationsBot(ILogger<OperationsBot> logger, IBotUpdateSwitch botUpdateSwitch)
    : BotFunctionBase(logger, botUpdateSwitch)
{
    protected override InteractionMode InteractionMode => InteractionMode.Operations;

    [Function("OperationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}