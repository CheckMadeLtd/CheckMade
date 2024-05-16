using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.RetryPolicies;

public interface IDbCommandRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class DbCommandRetryPolicy(ILogger<RetryPolicyBase> logger) 
    : RetryPolicyBase(1, "Execute Database Command", logger), IDbCommandRetryPolicy;
