using CheckMade.Common.Model.Enums;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.ByBotType;

public interface IOperationsRequestProcessor : IRequestProcessor;

public class OperationsRequestProcessor(IMessageRepository repo) : IOperationsRequestProcessor
{
    public async Task<Attempt<OutputDto>> ProcessRequestAsync(InputMessageDto inputMessage)
    {
        try
        {
            await repo.AddOrThrowAsync(inputMessage);
        }
        catch (Exception ex)
        {
            return Attempt<OutputDto>.Fail(new Error(Exception: ex));
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
                    Ui("Welcome to the CheckMade {0} Bot! ", BotType.Operations),
                    IRequestProcessor.SeeValidBotCommandsInstruction)),
            
            (int) OperationsBotCommands.NewIssue => OutputDto.Create(
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
            
            (int) OperationsBotCommands.NewAssessment => OutputDto.Create(
                Ui("⛺ Please choose a camp."),
                new []{ "Camp1", "Camp2", "Camp3", "Camp4" }),
            
            // (int) OperationsBotCommands.Experimental => OutputDto.Create(
            //     Ui("Please go here:"));
            
            _ => OutputDto.Create(
                UiConcatenate(
                    Ui("Echo of a {0} BotCommand: ", BotType.Operations), 
                    UiNoTranslate(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault().ToString())))
        };
    }
    
    private static Attempt<OutputDto> ProcessMessageWithAttachment(
        // ReSharper disable once UnusedParameter.Local
        InputMessageDto inputMessage, AttachmentType type)
    {
        return OutputDto.Create(
            Ui("Echo from bot {0}: {1}", BotType.Operations, type));
    }
    
    private static Attempt<OutputDto> ProcessNormalResponseMessage(InputMessageDto inputMessage)
    {
        return OutputDto.Create(
            Ui("Echo from bot {0}: {1}",
                BotType.Operations, inputMessage.Details.Text.GetValueOrDefault()));
    }
}
