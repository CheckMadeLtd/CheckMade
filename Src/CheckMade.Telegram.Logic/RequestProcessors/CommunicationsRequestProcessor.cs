using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.BotOperations;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor : ICommunicationsRequestProcessor
{
    public Task<Attempt<OutputDto>> SafelyEchoAsync(InputMessageDto inputMessage)
    {
        return Attempt<OutputDto>.RunAsync(() =>
        {
            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
            {
                return Task.FromResult(new OutputDto(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0}Bot! ", BotType.Communications),
                        IRequestProcessor.SeeValidBotCommandsInstruction),
                    Option<IEnumerable<BotOperation>>.None(),
                    Option<IEnumerable<string>>.None()));
            }

            return Task.FromResult(new OutputDto(
                Ui("Echo from bot {0}: {1}", BotType.Communications,
                    inputMessage.Details.Text.GetValueOrDefault()),
                Option<IEnumerable<BotOperation>>.None(),
                Option<IEnumerable<string>>.None()));
        });
    }
}