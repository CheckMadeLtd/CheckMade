using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor : INotificationsRequestProcessor
{
    public Task<string> EchoAsync(InputMessage message)
    {
        return Task.FromResult($"Echo from bot Notifications: {message.Details.Text}");
    }
}