using System.Net;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Endpoints;

public abstract class BotFunctionBase(ILogger logger, IBotUpdateHandler botUpdateHandler)
{
    protected abstract BotType BotType { get; }

    protected async Task<HttpResponseData> ProcessRequestAsync(HttpRequestData request)
    {
        logger.LogInformation("C# HTTP trigger function processed a request");
        
        try
        {
            var body = await request.ReadAsStringAsync() 
                       ?? throw new InvalidOperationException(
                           "The incoming HttpRequestData couldn't be serialized");
            
            var update = JsonConvert.DeserializeObject<Update>(body);
            
            if (update is null)
            {
                logger.LogWarning("Unable to deserialize Update object");
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }

            await botUpdateHandler.HandleUpdateAsync(update, BotType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing the request.");
            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        return request.CreateResponse(HttpStatusCode.OK);
    }
}