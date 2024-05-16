using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor : INotificationsRequestProcessor
{
    public Task<Attempt<string>> SafelyEchoAsync(InputMessage message)
    {
        return Task.FromResult(Attempt<string>.Run(() => 
            $"Echo from bot Notifications: {message.Details.Text.GetValueOrDefault()}"));
    }
}