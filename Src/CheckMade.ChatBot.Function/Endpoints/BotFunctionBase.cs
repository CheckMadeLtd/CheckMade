using System.Net;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using System.Text.Json;
using Telegram.Bot;

namespace CheckMade.ChatBot.Function.Endpoints;

public abstract class BotFunctionBase(ILogger logger, IBotUpdateSwitch botUpdateSwitch)
{
    protected abstract InteractionMode InteractionMode { get; }

    protected async Task<HttpResponseData> ProcessRequestAsync(HttpRequestData request)
    {
        logger.LogTrace("C# HTTP trigger function processed a request");

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

            var update = JsonSerializer.Deserialize<Update>(body, JsonBotAPI.Options);
            
            if (update is null)
            {
                logger.LogError("Unable to deserialize Update object");
                return defaultOkResponse;
            }

            // Any failure wrapped in Result would have been logged already e.g. in UpdateHandler
            // Only unhandled/unwrapped Exceptions would then bubble up here and lead to the catch block below, 
            await botUpdateSwitch.SwitchUpdateAsync(update, InteractionMode);

            return defaultOkResponse;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unhandled exception occurred while processing the request.");
            return defaultOkResponse;
        }
    }
}