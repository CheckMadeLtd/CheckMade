using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.ByBotType;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor : ICommunicationsRequestProcessor
{
    public Task<Attempt<OutputDto>> ProcessRequestAsync(InputMessageDto inputMessage)
    {
        return Attempt<OutputDto>.RunAsync(() =>
        {
            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
            {
                return Task.FromResult(OutputDto.Create(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0}Bot! ", BotType.Communications),
                        IRequestProcessor.SeeValidBotCommandsInstruction)));
            }

            return Task.FromResult(OutputDto.Create(
                Ui("Echo from bot {0}: {1}", BotType.Communications,
                    inputMessage.Details.Text.GetValueOrDefault())));
        });
    }
}