using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.Telegram.Function.Endpoints;

public class CommunicationsBot(ILogger<CommunicationsBot> logger, UpdateHandler updateHandler)
    : BotFunctionBase(logger, updateHandler)
{
    protected override BotType BotType => BotType.Communications;

    [Function("CommunicationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}
