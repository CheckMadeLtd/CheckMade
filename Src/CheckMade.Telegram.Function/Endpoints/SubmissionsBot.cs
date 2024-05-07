using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.Telegram.Function.Endpoints;

public class SubmissionsBot(ILogger<SubmissionsBot> logger, UpdateService updateService)
    : BotFunctionBase(logger, updateService)
{
    protected override BotType BotType => BotType.Submissions;

    [Function("SubmissionsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}