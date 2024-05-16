using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.RetryPolicies;

public interface IDbOpenRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class DbOpenRetryPolicy(ILogger<RetryPolicyBase> logger) 
    : RetryPolicyBase(5, "Open Database Connection", logger), IDbOpenRetryPolicy;
