using CheckMade.Common.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor(IUiTranslator translator) : INotificationsRequestProcessor
{
    public Task<Attempt<string>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return Attempt<string>.RunAsync(() => 
            Task.FromResult(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode 
            ? translator.Translate(UiConcatenate(
                Ui("Willkommen zum {0} Bot! ", BotType.Notifications), 
                IRequestProcessor.WelcomeToBotMenuInstruction)) 
            : translator.Translate(
                Ui($"Echo from bot Notifications: {inputMessage.Details.Text.GetValueOrDefault()}"))));
    }
}