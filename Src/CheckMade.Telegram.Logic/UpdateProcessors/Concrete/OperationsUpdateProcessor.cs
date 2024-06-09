using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Enums;
using CheckMade.Common.Model.Tlg;
using CheckMade.Common.Model.Tlg.Updates;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.UpdateProcessors.Concrete;

public interface IOperationsUpdateProcessor : IUpdateProcessor;

public class OperationsUpdateProcessor(
        ITlgUpdateRepository updateRepo,
        IRoleRepository roleRepo,
        ITlgClientPortToRoleMapRepository clientPortToRoleMapRepo) 
    : IOperationsUpdateProcessor
{
    private readonly Func<TlgClientPort, ITlgClientPortToRoleMapRepository, Task<bool>>
        _isTlgClientPortOfIncomingUpdateMappedToRoleAsync = async (updatePort, mapRepo) =>
        {
            IReadOnlyList<TlgClientPortToRoleMap> tlgClientPortToRoleMap =
                (await mapRepo.GetAllAsync()).ToList().AsReadOnly();
            
            // ToDo: fix to actual algorithm
            return tlgClientPortToRoleMap
                .FirstOrDefault(map => map.ClientPort.ChatId == updatePort.ChatId) != null;
        };
    
    public async Task<IReadOnlyList<OutputDto>> ProcessUpdateAsync(Result<TlgUpdate> tlgUpdate)
    {
        return await tlgUpdate.Match(
            async successfulUpdate =>
            {
                IReadOnlyList<Role> allRoles = (await roleRepo.GetAllAsync()).ToList().AsReadOnly();
                await updateRepo.AddAsync(successfulUpdate);

                var updatePort = new TlgClientPort(successfulUpdate.UserId, successfulUpdate.ChatId);
                
                return await _isTlgClientPortOfIncomingUpdateMappedToRoleAsync(updatePort, clientPortToRoleMapRepo) 
                    
                    ? successfulUpdate switch 
                    {
                        { Details.BotCommandEnumCode.IsSome: true } => ProcessBotCommand(successfulUpdate, allRoles),
                        
                        { Details.AttachmentType.IsSome: true } => ProcessMessageWithAttachment(
                            successfulUpdate, successfulUpdate.Details.AttachmentType.GetValueOrThrow()),
                        
                        _ => ProcessNormalResponseMessage(successfulUpdate)
                    } 
                    
                    : new List<OutputDto>{ new() { Text = IUpdateProcessor.AuthenticateWithToken } };
            },
            error => Task.FromResult<IReadOnlyList<OutputDto>>([ new OutputDto { Text = error } ])
        );
    }

    private static IReadOnlyList<OutputDto> ProcessBotCommand(
        TlgUpdate tlgUpdate,
        IReadOnlyList<Role> allRoles)
    {
        var currentBotCommand = tlgUpdate.Details.BotCommandEnumCode.GetValueOrThrow();
        
        return currentBotCommand switch
        {
            TlgStart.CommandCode => [
                new OutputDto
                {
                    Text = UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", TlgBotType.Operations),
                        IUpdateProcessor.SeeValidBotCommandsInstruction) 
                }
            ],
            
            (int) OperationsBotCommands.NewIssue => [
                new OutputDto
                {
                    LogicalPort = new TlgLogicPort(allRoles[0], TlgBotType.Operations),
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
                    LogicalPort = new TlgLogicPort(allRoles[0], TlgBotType.Operations),
                    Text = Ui("⛺ Please choose a camp."),
                    PredefinedChoices = new[] { "Camp1", "Camp2", "Camp3", "Camp4" } 
                }
            ],
            
            _ => new List<OutputDto>{ new()
                {
                    Text = UiConcatenate(
                        Ui("Echo of a {0} BotCommand: ", TlgBotType.Operations), 
                        UiNoTranslate(currentBotCommand.ToString())) 
                }
            }
        };
    }
    
    private static IReadOnlyList<OutputDto> ProcessMessageWithAttachment(
        // ReSharper disable UnusedParameter.Local
        TlgUpdate tlgUpdate, AttachmentType type)
    {
        return new List<OutputDto> { new()
            {
                Text = UiNoTranslate("Here, echo of your attachment."),
                Attachments = new List<OutputAttachmentDetails>
                {
                    new(tlgUpdate.Details.AttachmentInternalUri.GetValueOrThrow(), 
                        type, Option<UiString>.None())
                }
            }
        };
    }
    
    private static IReadOnlyList<OutputDto> ProcessNormalResponseMessage(TlgUpdate tlgUpdate)
    {
        // Temp, for testing purposes only
        if (tlgUpdate.Details.Text.GetValueOrDefault() == "n")
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
