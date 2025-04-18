using System.Data.Common;
using Microsoft.Extensions.Logging;
using Polly;

namespace CheckMade.Common.Utils.RetryPolicies;

public interface IDbCommandRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public sealed class DbCommandRetryPolicy : RetryPolicyBase, IDbCommandRetryPolicy
{
    public DbCommandRetryPolicy(ILogger<RetryPolicyBase> logger) : base("Execute Database Command", logger)
    {
        const int retryCount = 3;

        Policy = Polly.Policy
            .Handle<DbException>()
            .WaitAndRetryAsync(retryCount,
                static retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                (exception, timeSpan, retryAttempt, _) => LogError(exception, timeSpan, retryAttempt));
    }
}
