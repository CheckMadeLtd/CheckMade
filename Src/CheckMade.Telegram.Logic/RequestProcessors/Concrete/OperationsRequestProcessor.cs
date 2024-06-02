using CheckMade.Common.Interfaces;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.Concrete;

public interface IOperationsRequestProcessor : IRequestProcessor;

public class OperationsRequestProcessor(IMessageRepository repo) : IOperationsRequestProcessor
{
    public async Task<Attempt<IReadOnlyList<OutputDto>>> ProcessRequestAsync(InputMessageDto inputMessage)
    {
        try
        {
            await repo.AddOrThrowAsync(inputMessage);
        }
        catch (Exception ex)
        {
            return Attempt<IReadOnlyList<OutputDto>>.Fail(new Error(Exception: ex));
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

    private static Attempt<IReadOnlyList<OutputDto>> ProcessBotCommand(InputMessageDto inputMessage)
    {
        return inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() switch
        {
            Start.CommandCode => [
                OutputDto.Create(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Operations),
                        IRequestProcessor.SeeValidBotCommandsInstruction))
            ],
            
            (int) OperationsBotCommands.NewIssue => [
                OutputDto.Create(
                    new OutputDestination(BotType.Operations, new Role("token", RoleType.SanitaryOps_Admin)),
                    Ui("What type of issue?"),
                    new[]
                    {
                        DomainCategory.SanitaryOps_IssueCleanliness,
                        DomainCategory.SanitaryOps_IssueTechnical,
                        DomainCategory.SanitaryOps_IssueConsumable
                    },
                    new[]
                    {
                        ControlPrompts.Save
                    })
            ],
            
            // Testing ReplyKeyboard
            (int) OperationsBotCommands.NewAssessment => [
                OutputDto.Create(
                    new OutputDestination(BotType.Operations, new Role("token", RoleType.SanitaryOps_Admin)),
                    Ui("⛺ Please choose a camp."),
                    new[] { "Camp1", "Camp2", "Camp3", "Camp4" })
            ],
            
            _ => new List<OutputDto>{ OutputDto.Create(
                UiConcatenate(
                    Ui("Echo of a {0} BotCommand: ", BotType.Operations), 
                    UiNoTranslate(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault().ToString())))
            }
        };
    }
    
    private static Attempt<IReadOnlyList<OutputDto>> ProcessMessageWithAttachment(
        // ReSharper disable UnusedParameter.Local
        InputMessageDto inputMessage, AttachmentType type)
    {
        return new List<OutputDto>();
    }
    
    private static Attempt<IReadOnlyList<OutputDto>> ProcessNormalResponseMessage(InputMessageDto inputMessage)
    {
        return new List<OutputDto>();
    }
}
