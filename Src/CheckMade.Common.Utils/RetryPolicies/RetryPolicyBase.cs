using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace CheckMade.Common.Utils.RetryPolicies;

public abstract class RetryPolicyBase
{
    private readonly AsyncRetryPolicy _policy;

    protected RetryPolicyBase(int retryCount, string errorType, ILogger<RetryPolicyBase> logger)
    {
        _policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                (exception, timeSpan, retryAttempt, _) =>
                {
                    logger.LogError($"'{errorType}' error occurred at attempt no. {retryAttempt}. " +
                                    $"Exception type: '{exception.GetType()}'. " +
                                    $"Exception message: '{exception.Message}'. " +
                                    $"Attempting next time in {timeSpan.TotalSeconds} seconds...");
                });
    }

    public Task ExecuteAsync(Func<Task> action)
    {
        return _policy.ExecuteAsync(action);
    }
}