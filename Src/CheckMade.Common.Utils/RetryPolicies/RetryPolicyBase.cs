using Polly;
using Polly.Retry;

namespace CheckMade.Common.Utils.RetryPolicies;

public abstract class RetryPolicyBase
{
    private readonly AsyncRetryPolicy _policy;

    protected RetryPolicyBase(int retryCount, string errorType)
    {
        _policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                (exception, timeSpan, retryAttempt, _) =>
                {
                    // Console.Error ignored by xUnit (who only works with ITestOutputHelper) but should work for prd.
                    Console.Error.WriteLine($"'{errorType}' error occurred at attempt no. {retryAttempt}. " +
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