using System.Net;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Endpoints;

public abstract class BotFunctionBase(ILogger logger, IBotUpdateSwitch botUpdateSwitch)
{
    protected abstract BotType BotType { get; }

    protected async Task<HttpResponseData> ProcessRequestAsync(HttpRequestData request)
    {
        logger.LogInformation("C# HTTP trigger function processed a request");

        // IMPORTANT: Do NOT Use anything but HttpStatusCode.Ok (200) !!
        /* Any other response sends the Telegram Server into an endless loop reattempting the failed Update and
         paralysing the Bot! This also makes sense: just because my code can't process a certain type of update
         doesn't mean that there is something wrong with the way Telegram Server send me the request. */
        var defaultOkResponse = request.CreateResponse(HttpStatusCode.OK);
        
        try
        {
            var body = await request.ReadAsStringAsync() 
                       ?? throw new InvalidOperationException(
                           "The incoming HttpRequestData couldn't be serialized");

            var update = JsonConvert.DeserializeObject<Update>(body);

            if (update is null)
            {
                logger.LogError("Unable to deserialize Update object");
                return defaultOkResponse;
            }

            var updateHandlingOutcome = await botUpdateSwitch.SafelyHandleUpdateAsync(update, BotType);
            
            return updateHandlingOutcome.Match(
            _ => defaultOkResponse,
            ex =>
            {
                logger.LogWarning(ex, $"Can't process this kind of update. Message: {ex.Message}");
                return defaultOkResponse;
            });

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing the request.");
            return defaultOkResponse;
        }
    }
}