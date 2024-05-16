using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.RetryPolicies;

public interface INetworkRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class NetworkRetryPolicy(ILogger<RetryPolicyBase> logger) 
    : RetryPolicyBase(5, "Network", logger), INetworkRetryPolicy;