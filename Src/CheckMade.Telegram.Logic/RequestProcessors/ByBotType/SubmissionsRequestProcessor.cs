using CheckMade.Common.Model;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
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
                Option<IEnumerable<ControlPrompts>>.None(),
                Option<IEnumerable<DomainCategory>>.None(), 
                Option<IEnumerable<string>>.None()),
            
            (int) SubmissionsBotCommands.NewIssue => new OutputDto(
                Ui("Ok tell me more about the problem!"),
                Option<IEnumerable<ControlPrompts>>.None(),
                new []
                {
                    DomainCategory.SanitaryOpsIssueCleanliness,
                    DomainCategory.SanitaryOpsIssueTechnical,
                    DomainCategory.SanitaryOpsIssueConsumable
                },
                Option<IEnumerable<string>>.None()),
            
            _ => new OutputDto(
                UiConcatenate(
                    Ui("Echo of a {0} BotCommand: ", BotType.Submissions), 
                    UiNoTranslate(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault().ToString())), 
                Option<IEnumerable<ControlPrompts>>.None(),
                Option<IEnumerable<DomainCategory>>.None(), 
                Option<IEnumerable<string>>.None())
        };
    }
    
    private static Attempt<OutputDto> ProcessMessageWithAttachment(
        // ReSharper disable once UnusedParameter.Local
        InputMessageDto inputMessage, AttachmentType type)
    {
        return new OutputDto(
            Ui("Echo from bot {0}: {1}", BotType.Submissions, type),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<DomainCategory>>.None(), 
            Option<IEnumerable<string>>.None());
    }
    
    private static Attempt<OutputDto> ProcessNormalResponseMessage(InputMessageDto inputMessage)
    {
        return new OutputDto(
            Ui("Echo from bot {0}: {1}",
                BotType.Submissions, inputMessage.Details.Text.GetValueOrDefault()),
            Option<IEnumerable<ControlPrompts>>.None(), 
            Option<IEnumerable<DomainCategory>>.None(), 
            Option<IEnumerable<string>>.None());
    }
}
