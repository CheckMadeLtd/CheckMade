using CheckMade.Common.LangExt.MonadicWrappers;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor : INotificationsRequestProcessor
{
    public Task<Attempt<string>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return Attempt<string>.RunAsync(() => 
            Task.FromResult(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode 
            ? string.Format(IRequestProcessor.WelcomeToBot, BotType.Notifications) 
            : Ui($"Echo from bot Notifications: {inputMessage.Details.Text.GetValueOrDefault()}")));
    }
}