using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using CheckMade.ChatBot.Telegram.UpdateHandling;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.ChatBot.Telegram.Function;

public sealed class TelegramBotFunction(
    ILogger<TelegramBotFunction> logger, 
    IBotUpdateSwitch botUpdateSwitch) 
    : IBotFunction
{
    public async Task<HttpResponseData> ProcessRequestAsync(
        HttpRequestData request,
        ConcurrentDictionary<int, byte> currentlyProcessingUpdateIds,
        InteractionMode interactionMode)
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
            
            // Avoids duplicate processing when Telegram reattempts update delivery due to too-slow processing, which
            // can happen e.g. due to unhandled exceptions or retry policies. As recommended by Wizou. See also:
            // https://telegrambots.github.io/book/3/updates/webhook.html#updates-are-posted-sequentially-to-your-webapp
            if (!currentlyProcessingUpdateIds.TryAdd(update.Id, 0))
            {
                logger.LogTrace($"Already processing update with ID {update.Id} for {interactionMode}");
                return defaultOkResponse;
            }
            
            try
            {
                // Any failure wrapped in Result would have been logged already e.g. in UpdateHandler
                // Only unhandled/unwrapped Exceptions would then bubble up here and lead to the catch block below, 
                await botUpdateSwitch.SwitchUpdateAsync(update, interactionMode);
            }
            finally // any exception thrown within try would still be caught in the outer catch block below. 
            {
                currentlyProcessingUpdateIds.TryRemove(update.Id, out _);
                logger.LogTrace($"Finished processing update {update.Id} for {interactionMode}");
            }

            return defaultOkResponse;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unhandled exception occurred while processing the request.");
            return defaultOkResponse;
        }
    }
}