using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.LangExt.InputValidator;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

using static UserAuthWorkflow.States;

internal interface IUserAuthWorkflow : IWorkflow
{
    Task<UserAuthWorkflow.States> DetermineCurrentStateAsync(TlgAgent tlgAgent);
}

internal class UserAuthWorkflow(
        IRoleRepository roleRepo,
        ITlgAgentRoleBindingsRepository tlgAgentRoleBindingsRepo,
        IWorkflowUtils workflowUtils)
    : IUserAuthWorkflow
{
    private static readonly OutputDto EnterTokenPrompt = new()
    {
        Text = Ui("🌀 Please enter your role token (format '{0}'): ", GetTokenFormatExample())
    };

    public async Task<Result<IReadOnlyCollection<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        var inputText = tlgInput.Details.Text.GetValueOrDefault();
        
        return await DetermineCurrentStateAsync(tlgInput.TlgAgent) switch
        {
            Initial => new List<OutputDto> { EnterTokenPrompt },
            
            ReceivedTokenSubmissionAttempt => IsValidToken(inputText) switch
            {
                true => await TokenExists(tlgInput.Details.Text.GetValueOrDefault()) switch
                {
                    true => await AuthenticateUserAsync(tlgInput),
                    
                    false => [ new OutputDto
                        {
                            Text = Ui("This is an unknown token. Try again...")
                        },
                        EnterTokenPrompt ]
                },
                false => [ new OutputDto
                    {
                        Text = Ui("Bad token format! Try again...")
                    },
                    EnterTokenPrompt ]
            },
            
            _ => Result<IReadOnlyCollection<OutputDto>>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(UserAuthWorkflow)}"))
        };
    }
    
    public async Task<States> DetermineCurrentStateAsync(TlgAgent tlgAgent)
    {
        var allRelevantInputs = await workflowUtils.GetAllInputsOfTlgAgentInCurrentRoleAsync(tlgAgent);
        
        var lastTextSubmitted = allRelevantInputs
            .LastOrDefault(i => i.InputType == TlgInputType.TextMessage);

        return lastTextSubmitted switch
        {
            null => Initial,
            _ => ReceivedTokenSubmissionAttempt,
        };
    }

    private async Task<bool> TokenExists(string tokenAttempt) =>
        (await roleRepo.GetAllAsync()).Any(role => role.Token == tokenAttempt);

    private async Task<List<OutputDto>> AuthenticateUserAsync(TlgInput tokenInputAttempt)
    {
        var inputText = tokenInputAttempt.Details.Text.GetValueOrThrow();
        var originatingMode = tokenInputAttempt.TlgAgent.Mode;
        var preExistingTlgAgentRoles = workflowUtils.GetAllTlgAgentRoles();
        
        var outputs = new List<OutputDto>();
        
        var newTlgAgentRoleForOriginatingMode = new TlgAgentRoleBind(
            (await roleRepo.GetAllAsync()).First(r => r.Token == inputText),
            tokenInputAttempt.TlgAgent with { Mode = originatingMode },
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var preExistingActiveTlgAgentRole = FirstOrDefaultPreExistingActiveTlgAgentRoleMode(originatingMode);

        if (preExistingActiveTlgAgentRole != null)
        {
            await tlgAgentRoleBindingsRepo.UpdateStatusAsync(preExistingActiveTlgAgentRole, DbRecordStatus.Historic);
            
            outputs.Add(new OutputDto
            {
                Text = Ui("""
                          Warning: you were already authenticated with this token in another {0} chat. 
                          This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                          """, 
                    originatingMode,
                    newTlgAgentRoleForOriginatingMode.Role.RoleType,
                    newTlgAgentRoleForOriginatingMode.Role.LiveEvent.Name)
            });
        }
        
        outputs.Add(new OutputDto
        {
            Text = Ui("""
                      {0}, welcome to the CheckMade ChatBot!
                      You have successfully authenticated as a {1} at live-event {2}.
                      """, 
                newTlgAgentRoleForOriginatingMode.Role.User.FirstName,
                newTlgAgentRoleForOriginatingMode.Role.RoleType,
                newTlgAgentRoleForOriginatingMode.Role.LiveEvent.Name)
        });

        outputs.Add(new OutputDto
        {
            Text = IInputProcessor.SeeValidBotCommandsInstruction
        });

        var tlgAgentRolesToAdd = new List<TlgAgentRoleBind> { newTlgAgentRoleForOriginatingMode };
        
        var isInputTlgAgentPrivateChat = 
            tokenInputAttempt.TlgAgent.ChatId == tokenInputAttempt.TlgAgent.UserId;

        if (isInputTlgAgentPrivateChat)
        {
            AddTlgAgentRolesForOtherNonOriginatingAndVirginModes();
        }
        
        await tlgAgentRoleBindingsRepo.AddAsync(tlgAgentRolesToAdd);
        
        return outputs;
        
        TlgAgentRoleBind? FirstOrDefaultPreExistingActiveTlgAgentRoleMode(InteractionMode mode) =>
        preExistingTlgAgentRoles.FirstOrDefault(arb => 
            arb.Role.Token == inputText &&
            arb.TlgAgent.Mode == mode && 
            arb.Status == DbRecordStatus.Active);

        void AddTlgAgentRolesForOtherNonOriginatingAndVirginModes()
        {
            var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
            var nonOriginatingModes = allModes.Except(new [] { originatingMode });

            tlgAgentRolesToAdd.AddRange(
                from mode in nonOriginatingModes 
                where FirstOrDefaultPreExistingActiveTlgAgentRoleMode(mode) == null
                select newTlgAgentRoleForOriginatingMode with
                {
                    TlgAgent = newTlgAgentRoleForOriginatingMode.TlgAgent with { Mode = mode },
                    ActivationDate = DateTime.UtcNow
                });
        }
    }
    
    [Flags]
    internal enum States
    {
        Initial = 1,
        ReceivedTokenSubmissionAttempt = 1<<1,
    }
}