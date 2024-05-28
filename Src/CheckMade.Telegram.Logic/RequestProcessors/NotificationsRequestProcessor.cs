using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.BotPrompts;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface INotificationsRequestProcessor : IRequestProcessor;

public class NotificationsRequestProcessor : INotificationsRequestProcessor
{
    public Task<Attempt<OutputDto>> SafelyEchoAsync(InputMessageDto inputMessage)
    {
        return Attempt<OutputDto>.RunAsync(() =>
        {
            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
            {
                return Task.FromResult(new OutputDto(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0}Bot! ", BotType.Notifications),
                        IRequestProcessor.SeeValidBotCommandsInstruction),
                    Option<IEnumerable<BotPrompt>>.None(),
                    Option<IEnumerable<string>>.None()));
            }

            return Task.FromResult(new OutputDto(
                Ui("Echo from bot {0}: {1}", BotType.Notifications,
                    inputMessage.Details.Text.GetValueOrDefault()),
                Option<IEnumerable<BotPrompt>>.None(),
                Option<IEnumerable<string>>.None()));
        });
    }
}