namespace CheckMade.Common.Utils.RetryPolicies;

public interface INetworkRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class NetworkRetryPolicy() : RetryPolicyBase(5, "Network"), INetworkRetryPolicy;