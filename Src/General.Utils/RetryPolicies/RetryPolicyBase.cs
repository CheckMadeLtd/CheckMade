using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace General.Utils.RetryPolicies;

public abstract class RetryPolicyBase(string errorType, ILogger<RetryPolicyBase> logger)
{
    protected AsyncRetryPolicy? Policy;

    public Task ExecuteAsync(Func<Task> action)
    {
        return Policy!.ExecuteAsync(action);
    }

    protected void LogError(Exception ex, TimeSpan timeSpan, int retryAttempt)
    {
        logger.LogError($"'{errorType}' error occurred at attempt no. {retryAttempt}. " +
                        $"Exception type: '{ex.GetType()}'. " +
                        $"Exception message: '{ex.Message}'. " +
                        $"Attempting next time in {timeSpan.TotalSeconds} seconds...");
    }
}
