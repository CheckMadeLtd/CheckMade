using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.UpdateProcessors.Concrete;

public interface IOperationsUpdateProcessor : IUpdateProcessor;

public class OperationsUpdateProcessor(
        ITelegramUpdateRepository updateRepo,
        IRoleRepository roleRepo) 
    : IOperationsUpdateProcessor
{
    public async Task<IReadOnlyList<OutputDto>> ProcessUpdateAsync(Result<TelegramUpdate> telegramUpdate)
    {
        return await telegramUpdate.Match(
            async successfulUpdate =>
            {
                IReadOnlyList<Role> allRoles = (await roleRepo.GetAllAsync()).ToList().AsReadOnly();
                await updateRepo.AddAsync(successfulUpdate);
                
                return successfulUpdate switch
                {
                    { Details.BotCommandEnumCode.IsSome: true } => ProcessBotCommand(successfulUpdate, allRoles),
                    { Details.AttachmentType.IsSome: true } => ProcessMessageWithAttachment(
                        successfulUpdate, successfulUpdate.Details.AttachmentType.GetValueOrThrow()),
                    _ => ProcessNormalResponseMessage(successfulUpdate)
                };
            },
            error => Task.FromResult<IReadOnlyList<OutputDto>>([ OutputDto.Create(error) ])
        );
    }

    private static IReadOnlyList<OutputDto> ProcessBotCommand(
        TelegramUpdate telegramUpdate,
        IReadOnlyList<Role> allRoles)
    {
        var currentBotCommand = telegramUpdate.Details.BotCommandEnumCode.GetValueOrThrow();
        
        return currentBotCommand switch
        {
            Start.CommandCode => [
                OutputDto.Create(
                    UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Operations),
                        IUpdateProcessor.SeeValidBotCommandsInstruction))
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
                    UiNoTranslate(currentBotCommand.ToString())))
            }
        };
    }
    
    private static IReadOnlyList<OutputDto> ProcessMessageWithAttachment(
        // ReSharper disable UnusedParameter.Local
        TelegramUpdate telegramUpdate, AttachmentType type)
    {
        return new List<OutputDto>
        {
            OutputDto.Create(
                UiNoTranslate("Here, echo of your attachment."),
                new List<OutputAttachmentDetails>
                {
                    new(telegramUpdate.Details.AttachmentInternalUri.GetValueOrThrow(), 
                        type, 
                        Option<UiString>.None())
                }),
        };
    }
    
    private static IReadOnlyList<OutputDto> ProcessNormalResponseMessage(TelegramUpdate telegramUpdate)
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
