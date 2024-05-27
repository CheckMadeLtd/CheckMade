using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.BotResponsePrompts;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<Attempt<OutputDto>> SafelyEchoAsync(InputMessageDto inputMessage)
    {
        return await Attempt<OutputDto>.RunAsync(async () =>
        {
            await repo.AddOrThrowAsync(inputMessage);

            if (inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode)
                return new OutputDto(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0}Bot! ", BotType.Submissions), 
                        IRequestProcessor.SeeValidBotCommandsInstruction), 
                    Option<IEnumerable<BotResponsePrompt>>.None(), 
                    Option<IEnumerable<string>>.None());
            
            if (inputMessage.BotType is BotType.Submissions &&
                inputMessage.Details.BotCommandEnumCode.IsSome)
            {
                return new OutputDto(
                    UiConcatenate(
                        Ui("Echo of a {0} BotCommand: ", BotType.Submissions), 
                        UiNoTranslate(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault().ToString())),
                    Option<IEnumerable<BotResponsePrompt>>.None(), 
                    Option<IEnumerable<string>>.None());
            }

            return inputMessage.Details.AttachmentType.Match(
                
                type => new OutputDto(
                    Ui("Echo from bot {0}: {1}", BotType.Submissions, type),
                    Option<IEnumerable<BotResponsePrompt>>.None(), 
                    Option<IEnumerable<string>>.None()),
                
                () => new OutputDto(
                    Ui("Echo from bot {0}: {1}",
                        BotType.Submissions, inputMessage.Details.Text.GetValueOrDefault()),
                    Option<IEnumerable<BotResponsePrompt>>.None(), 
                    Option<IEnumerable<string>>.None())
                );
        });
    }
}
