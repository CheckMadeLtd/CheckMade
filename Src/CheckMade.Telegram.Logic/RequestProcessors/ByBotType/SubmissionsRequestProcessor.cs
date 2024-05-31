using CheckMade.Common.Model.Enums;
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
            Start.CommandCode => OutputDto.Create(
                UiConcatenate(
                    Ui("Welcome to the CheckMade {0}Bot! ", BotType.Submissions),
                    IRequestProcessor.SeeValidBotCommandsInstruction)),
            
            (int) SubmissionsBotCommands.NewIssue => OutputDto.Create(
                Ui("What type of issue?"),
                new []
                {
                    DomainCategory.SanitaryOps_IssueCleanliness,
                    DomainCategory.SanitaryOps_IssueTechnical,
                    DomainCategory.SanitaryOps_IssueConsumable
                },
                new []
                {
                    ControlPrompts.Save
                }),
            
            (int) SubmissionsBotCommands.NewAssessment => OutputDto.Create(
                Ui("⛺ Please choose a camp."),
                new []{ "Camp1", "Camp2", "Camp3", "Camp4" }),
            
            // (int) SubmissionsBotCommands.Experimental => OutputDto.Create(
            //     Ui("Please go here:"));
            
            _ => OutputDto.Create(
                UiConcatenate(
                    Ui("Echo of a {0} BotCommand: ", BotType.Submissions), 
                    UiNoTranslate(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault().ToString())))
        };
    }
    
    private static Attempt<OutputDto> ProcessMessageWithAttachment(
        // ReSharper disable once UnusedParameter.Local
        InputMessageDto inputMessage, AttachmentType type)
    {
        return OutputDto.Create(
            Ui("Echo from bot {0}: {1}", BotType.Submissions, type));
    }
    
    private static Attempt<OutputDto> ProcessNormalResponseMessage(InputMessageDto inputMessage)
    {
        return OutputDto.Create(
            Ui("Echo from bot {0}: {1}",
                BotType.Submissions, inputMessage.Details.Text.GetValueOrDefault()));
    }
}
