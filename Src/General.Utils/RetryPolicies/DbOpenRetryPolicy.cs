using System.Data.Common;
using Microsoft.Extensions.Logging;
using Polly;

namespace General.Utils.RetryPolicies;

public interface IDbOpenRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public sealed class DbOpenRetryPolicy : RetryPolicyBase, IDbOpenRetryPolicy
{
    public DbOpenRetryPolicy(ILogger<RetryPolicyBase> logger) : base("Open Database Connection", logger)
    {
        const int retryCount = 3;

        Policy = Polly.Policy
            .Handle<DbException>()
            .WaitAndRetryAsync(retryCount,
                static retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                (exception, timeSpan, retryAttempt, _) => LogError(exception, timeSpan, retryAttempt));
    }
}
