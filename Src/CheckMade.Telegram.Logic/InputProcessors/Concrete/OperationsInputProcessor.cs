using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Enums;
using CheckMade.Common.Model.Tlg;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.InputProcessors.Concrete;

public interface IOperationsInputProcessor : IInputProcessor;

public class OperationsInputProcessor(
        ITlgInputRepository inputRepo,
        IRoleRepository roleRepo,
        ITlgClientPortToRoleMapRepository clientPortToRoleMapRepo) 
    : IOperationsInputProcessor
{
    private readonly Func<TlgClientPort, ITlgClientPortToRoleMapRepository, Task<bool>>
        _isInputTlgClientPortMappedToRoleAsync = async (inputPort, mapRepo) =>
        {
            IReadOnlyList<TlgClientPortToRoleMap> tlgClientPortToRoleMap =
                (await mapRepo.GetAllAsync()).ToList().AsReadOnly();
            
            // ToDo: fix to actual algorithm
            return tlgClientPortToRoleMap
                .FirstOrDefault(map => map.ClientPort.ChatId == inputPort.ChatId) != null;
        };
    
    public async Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput)
    {
        return await tlgInput.Match(
            async input =>
            {
                IReadOnlyList<Role> allRoles = (await roleRepo.GetAllAsync()).ToList().AsReadOnly();
                await inputRepo.AddAsync(input);

                var inputPort = new TlgClientPort(input.UserId, input.ChatId);
                
                return await _isInputTlgClientPortMappedToRoleAsync(inputPort, clientPortToRoleMapRepo) 
                    
                    ? input switch 
                    {
                        { Details.BotCommandEnumCode.IsSome: true } => ProcessBotCommand(input, allRoles),
                        
                        { Details.AttachmentType.IsSome: true } => ProcessMessageWithAttachment(
                            input, input.Details.AttachmentType.GetValueOrThrow()),
                        
                        _ => ProcessNormalResponseMessage(input)
                    } 
                    
                    : new List<OutputDto>{ new() { Text = IInputProcessor.AuthenticateWithToken } };
            },
            error => Task.FromResult<IReadOnlyList<OutputDto>>([ new OutputDto { Text = error } ])
        );
    }

    private static IReadOnlyList<OutputDto> ProcessBotCommand(
        TlgInput tlgInput,
        IReadOnlyList<Role> allRoles)
    {
        var currentBotCommand = tlgInput.Details.BotCommandEnumCode.GetValueOrThrow();
        
        return currentBotCommand switch
        {
            TlgStart.CommandCode => [
                new OutputDto
                {
                    Text = UiConcatenate(
                        Ui("Welcome to the CheckMade {0} Bot! ", TlgBotType.Operations),
                        IInputProcessor.SeeValidBotCommandsInstruction) 
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
        TlgInput tlgInput, AttachmentType type)
    {
        return new List<OutputDto> { new()
            {
                Text = UiNoTranslate("Here, echo of your attachment."),
                Attachments = new List<OutputAttachmentDetails>
                {
                    new(tlgInput.Details.AttachmentInternalUri.GetValueOrThrow(), 
                        type, Option<UiString>.None())
                }
            }
        };
    }
    
    private static IReadOnlyList<OutputDto> ProcessNormalResponseMessage(TlgInput tlgInput)
    {
        // Temp, for testing purposes only
        if (tlgInput.Details.Text.GetValueOrDefault() == "n")
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
