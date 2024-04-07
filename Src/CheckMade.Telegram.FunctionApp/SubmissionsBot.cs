using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.FunctionApp;

public class TelegramBot(ILogger<TelegramBot> logger, UpdateService updateService)
{
    [Function("SubmissionsBot")]
    public async Task<HttpResponseData> 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
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

            await updateService.EchoAsync(update);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            logger.LogError("Exception: {Message}", e.Message);
        }

        return response;
    }
}