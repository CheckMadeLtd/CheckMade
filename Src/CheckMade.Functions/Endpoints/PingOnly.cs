using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CheckMade.Functions.Endpoints;

public sealed class PingOnly
{
    // Regular ping from cron-job.org prevents cheap Consumption-Plan Function from falling asleep 
    [Function("PingOnly")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData _)
    {
        return _.CreateResponse(HttpStatusCode.OK);
    }
}