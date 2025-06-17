using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;

namespace General.Utils.RetryPolicies;

public interface INetworkRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public sealed class NetworkRetryPolicy : RetryPolicyBase, INetworkRetryPolicy
{
    private static readonly Type[] ExceptionTypesToHandle =
    [
        typeof(HttpRequestException),
        typeof(TimeoutException),
        typeof(IOException),
        typeof(OperationCanceledException),
        typeof(WebException),
        typeof(SocketException)
    ];
    
    public NetworkRetryPolicy(ILogger<RetryPolicyBase> logger) : base("Network", logger)
    {
        const int retryCount = 5;

        Policy = Polly.Policy
            .Handle<Exception>(static ex => 
                IsTargetExceptionOrHasTargetInnerException(ex, ExceptionTypesToHandle))
            .WaitAndRetryAsync(retryCount,
                static retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                (exception, timeSpan, retryAttempt, _) => LogError(exception, timeSpan, retryAttempt));
    }
    
    private static bool IsTargetExceptionOrHasTargetInnerException(
        Exception? ex, IReadOnlyCollection<Type> targetTypes)
    {
        if (ex == null)
            return false;

        return 
            targetTypes.Any(type => type.IsInstanceOfType(ex)) || 
            IsTargetExceptionOrHasTargetInnerException(ex.InnerException, targetTypes);
    }
}
