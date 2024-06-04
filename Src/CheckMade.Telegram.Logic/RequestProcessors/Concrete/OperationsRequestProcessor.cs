using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;
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
                    new TelegramOutputDestination(allRoles[0], BotType.Operations),
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
                    new TelegramOutputDestination(allRoles[0], BotType.Operations),
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
        return new List<OutputDto>
        {
            OutputDto.Create(
                UiNoTranslate("Here, echo of your attachment."),
                new List<OutputAttachmentDetails>
                {
                    new(telegramUpdate.Details.AttachmentInternalUri.GetValueOrDefault(), type)
                }),
        };
    }
    
    private static Attempt<IReadOnlyList<OutputDto>> ProcessNormalResponseMessage(TelegramUpdate telegramUpdate)
    {
        // Temp, for testing purposes only
        if (telegramUpdate.Details.Text.GetValueOrDefault() == "n")
        {
            return new List<OutputDto>
            {
                OutputDto.Create(UiNoTranslate("Message1")),
                OutputDto.Create(UiNoTranslate("Message2")), 
                OutputDto.Create(
                    UiNoTranslate("Go here now:"),
                    new Geo(12.111, 34.007, Option<float>.None()))
            };
        }

        return new List<OutputDto>();
    }
}
