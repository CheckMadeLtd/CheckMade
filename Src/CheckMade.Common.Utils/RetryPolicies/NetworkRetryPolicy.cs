using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;

namespace CheckMade.Common.Utils.RetryPolicies;

public interface INetworkRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class NetworkRetryPolicy : RetryPolicyBase, INetworkRetryPolicy
{
    public NetworkRetryPolicy(ILogger<RetryPolicyBase> logger) : base("Network", logger)
    {
        const int retryCount = 5;

        Policy = Polly.Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<IOException>()
            .Or<OperationCanceledException>()
            .Or<WebException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                (exception, timeSpan, retryAttempt, _) => LogError(exception, timeSpan, retryAttempt));
    }
}
