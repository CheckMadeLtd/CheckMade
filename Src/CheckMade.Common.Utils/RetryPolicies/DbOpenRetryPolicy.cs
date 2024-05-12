namespace CheckMade.Common.Utils.RetryPolicies;

public interface IDbOpenRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class DbOpenRetryPolicy() : RetryPolicyBase(5, "Open Database Connection"), IDbOpenRetryPolicy;
