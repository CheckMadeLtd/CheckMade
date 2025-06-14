namespace CheckMade.Common.DomainModel.Interfaces.Utils;

public interface INetworkRetryPolicy
{
    Task ExecuteAsync(Func<Task> action);
}
