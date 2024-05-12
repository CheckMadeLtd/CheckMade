namespace CheckMade.Common.Utils.RetryPolicies;

public interface IDbCommandRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class DbCommandRetryPolicy() : RetryPolicyBase(1, "Execute Database Command"), IDbCommandRetryPolicy;
