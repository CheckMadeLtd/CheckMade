using System.Collections.Concurrent;
using CheckMade.Core.Model.Bot.Categories;
using Microsoft.Azure.Functions.Worker.Http;


namespace CheckMade.Bot.Telegram.Function;

public interface IBotFunction
{
    Task<HttpResponseData> ProcessRequestAsync(
        HttpRequestData request,
        ConcurrentDictionary<int, byte> currentlyProcessingUpdateIds,
        InteractionMode interactionMode);
}