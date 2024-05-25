using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor : INotificationsRequestProcessor
{
    public Task<Attempt<UiString>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return Attempt<UiString>.RunAsync(() => 
            Task.FromResult(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode 
            ? UiConcatenate(
                Ui("Willkommen zum {0} Bot! ", BotType.Notifications), 
                IRequestProcessor.WelcomeToBotMenuInstruction) 
            : Ui("Echo from bot \nNotifications: {0}", inputMessage.Details.Text.GetValueOrDefault())));
    }
}