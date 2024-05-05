using CheckMade.Telegram.Logic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CheckMade.Telegram.Function.Functions;

public class CommunicationsBot(ILogger<CommunicationsBot> logger, UpdateService updateService)
    : TelegramBotFunctionBase(logger, updateService)
{
    protected override BotType BotType => BotType.Communications;

    [Function("CommunicationsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        return await ProcessRequestAsync(request);
    }
}
