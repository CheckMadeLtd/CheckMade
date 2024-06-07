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
            error => Task.FromResult<IReadOnlyList<OutputDto>>([ new OutputDto { Text = error } ])
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
                new OutputDto
                {
                    Text = UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", BotType.Operations),
                        IUpdateProcessor.SeeValidBotCommandsInstruction) 
                }
            ],
            
            (int) OperationsBotCommands.NewIssue => [
                new OutputDto
                {
                    ExplicitDestination = new TelegramOutputDestination(allRoles[0], BotType.Operations),
                    Text = Ui("What type of issue?"),
                    DomainCategorySelection = new[]
                    {
                        DomainCategory.SanitaryOps_IssueCleanliness,
                        DomainCategory.SanitaryOps_IssueTechnical,
                        DomainCategory.SanitaryOps_IssueConsumable
                    },
                    ControlPromptsSelection = new[] { ControlPrompts.Save } 
                }
            ],
            
            // Testing ReplyKeyboard
            (int) OperationsBotCommands.NewAssessment => [
                new OutputDto
                {
                    ExplicitDestination = new TelegramOutputDestination(allRoles[0], BotType.Operations),
                    Text = Ui("⛺ Please choose a camp."),
                    PredefinedChoices = new[] { "Camp1", "Camp2", "Camp3", "Camp4" } 
                }
            ],
            
            _ => new List<OutputDto>{ new()
                {
                    Text = UiConcatenate(
                        Ui("Echo of a {0} BotCommand: ", BotType.Operations), 
                        UiNoTranslate(currentBotCommand.ToString())) 
                }
            }
        };
    }
    
    private static IReadOnlyList<OutputDto> ProcessMessageWithAttachment(
        // ReSharper disable UnusedParameter.Local
        TelegramUpdate telegramUpdate, AttachmentType type)
    {
        return new List<OutputDto> { new()
            {
                Text = UiNoTranslate("Here, echo of your attachment."),
                Attachments = new List<OutputAttachmentDetails>
                {
                    new(telegramUpdate.Details.AttachmentInternalUri.GetValueOrThrow(), 
                        type, Option<UiString>.None())
                } 
            }
        };
    }
    
    private static IReadOnlyList<OutputDto> ProcessNormalResponseMessage(TelegramUpdate telegramUpdate)
    {
        // Temp, for testing purposes only
        if (telegramUpdate.Details.Text.GetValueOrDefault() == "n")
        {
            return new List<OutputDto>
            {
                new() { Text = UiNoTranslate("Message1") },
                new() { Text = UiNoTranslate("Message2") }, 
                new() 
                { 
                    Text = UiNoTranslate("Go here now:"),
                    Location = new Geo(12.111, 34.007, Option<float>.None()) 
                }
            };
        }

        return new List<OutputDto>();
    }
}
