namespace CheckMade.Common.Utils.RetryPolicies;

public interface IDbOpenRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class DbOpenRetryPolicy() : RetryPolicyBase(3, "Open Database Connection"), IDbOpenRetryPolicy;
