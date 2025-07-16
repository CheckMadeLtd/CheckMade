using System.Collections.Concurrent;
using System.Net;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Bot.Telegram.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CheckMade.Functions.Endpoints;

public sealed class OperationsBot(IBotFunction botFunction)
{
    // Thread-safe collection for this static cache (ensuring atomic operations in case of multiple function instances)
    private static readonly ConcurrentDictionary<int, byte> UpdateIds = new();
    private static InteractionMode Mode => InteractionMode.Operations;

    [Function("OperationsBot")]
    public HttpResponseData 
        Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        // Not awaiting here on purpose, so that execution resumes as soon as any downstream 'await' actually waits!
        // This way we return 'OK' as soon as the synchronous code is done, i.e. almost instantly.
        // This means Telegram Server can send its next update, which would (potentially) be processed in parallel.
        // This corresponds to "sub-task" option described in:
        // https://telegrambots.github.io/book/3/updates/#sequential-vs-parallel-updates
        botFunction.ProcessRequestAsync(request, UpdateIds, Mode);

        return request.CreateResponse(HttpStatusCode.OK);
    }
}