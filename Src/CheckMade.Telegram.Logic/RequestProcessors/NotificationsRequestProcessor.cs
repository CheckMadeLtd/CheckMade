using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor : INotificationsRequestProcessor
{
    public Task<Attempt<UiString>> SafelyEchoAsync(InputMessageDto inputMessage)
    {
        return Attempt<UiString>.RunAsync(() => 
            Task.FromResult(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode 
            ? UiConcatenate(
                Ui("Welcome to the CheckMade {0}Bot! ", BotType.Notifications), 
                IRequestProcessor.SeeValidBotCommandsInstruction) 
            : Ui("Echo from bot {0}: {1}", BotType.Notifications, inputMessage.Details.Text.GetValueOrDefault())));
    }
}