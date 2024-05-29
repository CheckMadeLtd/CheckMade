using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.BotPrompts;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.ByBotType;

public interface ISubmissionsRequestProcessor : IRequestProcessor;

public class SubmissionsRequestProcessor(IMessageRepository repo) : ISubmissionsRequestProcessor
{
    public async Task<Attempt<OutputDto>> ProcessRequestAsync(InputMessageDto inputMessage)
    {
        try
        {
            await repo.AddOrThrowAsync(inputMessage);
        }
        catch (Exception ex)
        {
            return Attempt<OutputDto>.Fail(new Failure(Exception: ex));
        }

        return inputMessage switch
        {
            { Details.BotCommandEnumCode.IsSome: true } =>
                ProcessBotCommand(inputMessage),

            { Details.AttachmentType: { IsSome: true, Value: var type } } =>
                ProcessMessageWithAttachment(inputMessage, type),

            _ => ProcessNormalResponseMessage(inputMessage)
        };
    }

    private static Attempt<OutputDto> ProcessBotCommand(InputMessageDto inputMessage)
    {
        return inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() switch
        {
            Start.CommandCode => new OutputDto(
                UiConcatenate(
                    Ui("Welcome to the CheckMade {0}Bot! ", BotType.Submissions),
                    IRequestProcessor.SeeValidBotCommandsInstruction),
                Option<IEnumerable<EBotPrompts>>.None(),
                Option<IEnumerable<string>>.None()),
            
            // ToDo: I think need to have an Enum for all available BotPrompts after all - for typed access in code...
            // I think, I might be able to use the int of the Prompt Enum as the CallBack ID for the InlineReyplyButton!!!!
            
            // (int) SubmissionsBotCommands.Problem => new OutputDto(
            //     Ui("Ok tell me more about the problem!"),
            //     new [] {  })
            
            _ => new OutputDto(
                UiConcatenate(
                    Ui("Echo of a {0} BotCommand: ", BotType.Submissions), 
                    UiNoTranslate(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault().ToString())), 
                Option<IEnumerable<EBotPrompts>>.None(), 
                Option<IEnumerable<string>>.None())
        };
    }
    
    private static Attempt<OutputDto> ProcessMessageWithAttachment(
        // ReSharper disable once UnusedParameter.Local
        InputMessageDto inputMessage, AttachmentType type)
    {
        return new OutputDto(
            Ui("Echo from bot {0}: {1}", BotType.Submissions, type),
            Option<IEnumerable<EBotPrompts>>.None(), 
            Option<IEnumerable<string>>.None());
    }
    
    private static Attempt<OutputDto> ProcessNormalResponseMessage(InputMessageDto inputMessage)
    {
        return new OutputDto(
            Ui("Echo from bot {0}: {1}",
                BotType.Submissions, inputMessage.Details.Text.GetValueOrDefault()),
            Option<IEnumerable<EBotPrompts>>.None(), 
            Option<IEnumerable<string>>.None());
    }
}
