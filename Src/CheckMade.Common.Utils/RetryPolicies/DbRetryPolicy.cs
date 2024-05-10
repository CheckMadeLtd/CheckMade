namespace CheckMade.Common.Utils.RetryPolicies;

public interface IDbRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}

public class DbRetryPolicy() : RetryPolicyBase(2, "Database"), IDbRetryPolicy;
