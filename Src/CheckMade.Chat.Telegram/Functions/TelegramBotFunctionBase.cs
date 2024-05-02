using System.Net;
using CheckMade.Chat.Logic;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace CheckMade.Chat.Telegram.Functions;

public abstract class TelegramBotFunctionBase(ILogger logger, UpdateService updateService)
{
    protected abstract BotType BotType { get; }

    protected async Task<HttpResponseData> ProcessRequestAsync(HttpRequestData request)
    {
        logger.LogInformation("C# HTTP trigger function processed a request");
        var response = request.CreateResponse(HttpStatusCode.OK);
        try
        {
            var body = await request.ReadAsStringAsync() ?? throw new ArgumentNullException(nameof(request));
            var update = JsonConvert.DeserializeObject<Update>(body);
            if (update is null)
            {
                logger.LogWarning("Unable to deserialize Update object");
                return response;
            }

            await updateService.HandleUpdateAsync(update, BotType);
        }
        catch (Exception e)
        {
            logger.LogError("Exception: {Message}", e.Message);
        }

        return response;
    }
}