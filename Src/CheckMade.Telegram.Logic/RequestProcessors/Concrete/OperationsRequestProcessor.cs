using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors.Concrete;

public interface IOperationsRequestProcessor : IRequestProcessor;

public class OperationsRequestProcessor(
        ITelegramUpdateRepository updateRepo,
        IRoleRepository roleRepo) 
    : IOperationsRequestProcessor
{
    public async Task<Attempt<IReadOnlyList<OutputDto>>> ProcessRequestAsync(TelegramUpdate telegramUpdate)
    {
        IReadOnlyList<Role> allRoles;
        
        try
        {
            await updateRepo.AddOrThrowAsync(telegramUpdate);
            allRoles = (await roleRepo.GetAllOrThrowAsync()).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            return Attempt<IReadOnlyList<OutputDto>>.Fail(new Error(Exception: ex));
        }

        return telegramUpdate switch
        {
            { Details.BotCommandEnumCode.IsSome: true } =>
                ProcessBotCommand(telegramUpdate, allRoles),

            { Details.AttachmentType: { IsSome: true, Value: var type } } =>
                ProcessMessageWithAttachment(telegramUpdate, type),

            _ => ProcessNormalResponseMessage(telegramUpdate)
        };
    }

    // ToDo: Need to handle the case where the update sending ChatId has no mapping to RoleBotType
    // ==> Maybe leave out explicit OutputDestination in that case? Yes! It's optional now!
    // And in that case, SendOut will simply send to the updateReceivingBotClient and updateReceivingChatId by default! 
    // And whatever the user sent/did, the answer in that case here is the same: please log in !! 
    
    private static Attempt<IReadOnlyList<OutputDto>> ProcessBotCommand(
        TelegramUpdate telegramUpdate,
        IReadOnlyList<Role> allRoles)
    {
        return telegramUpdate.Details.BotCommandEnumCode.GetValueOrDefault() switch
        {
            Start.CommandCode => [
                OutputDto.Create(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Operations),
                        IRequestProcessor.SeeValidBotCommandsInstruction))
            ],
            
            (int) OperationsBotCommands.NewIssue => [
                OutputDto.Create(
                    new OutputDestination(BotType.Operations, allRoles[0]),
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
                    new OutputDestination(BotType.Operations, allRoles[0]),
                    Ui("⛺ Please choose a camp."),
                    new[] { "Camp1", "Camp2", "Camp3", "Camp4" })
            ],
            
            _ => new List<OutputDto>{ OutputDto.Create(
                UiConcatenate(
                    Ui("Echo of a {0} BotCommand: ", BotType.Operations), 
                    UiNoTranslate(telegramUpdate.Details.BotCommandEnumCode.GetValueOrDefault().ToString())))
            }
        };
    }
    
    private static Attempt<IReadOnlyList<OutputDto>> ProcessMessageWithAttachment(
        // ReSharper disable UnusedParameter.Local
        TelegramUpdate telegramUpdate, AttachmentType type)
    {
        return new List<OutputDto>();
    }
    
    private static Attempt<IReadOnlyList<OutputDto>> ProcessNormalResponseMessage(TelegramUpdate telegramUpdate)
    {
        return new List<OutputDto>();
    }
}
