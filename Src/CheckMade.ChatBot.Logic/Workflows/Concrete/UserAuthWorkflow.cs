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
    UserAuthWorkflow.States DetermineCurrentState(IReadOnlyCollection<TlgInput> tlgAgentInputHistory);
}

internal class UserAuthWorkflow(
        IRolesRepository rolesRepo,
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILogicUtils logicUtils)
    : IUserAuthWorkflow
{
    internal static readonly OutputDto EnterTokenPrompt = new()
    {
        Text = Ui("🌀 Please enter your role token (format '{0}'): ", GetTokenFormatExample())
    };

    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        return DetermineCurrentState(inputHistory) == ReceivedTokenSubmissionAttempt;
    }

    public async Task<Result<IReadOnlyCollection<OutputDto>>> GetNextOutputAsync(TlgInput currentInput)
    {
        var inputText = currentInput.Details.Text.GetValueOrDefault();
        
        var tlgAgentInputHistory = 
            await logicUtils.GetAllCurrentInputsAsync(currentInput.TlgAgent);
        
        return DetermineCurrentState(tlgAgentInputHistory) switch
        {
            Initial => new List<OutputDto> { EnterTokenPrompt },
            
            ReceivedTokenSubmissionAttempt => IsValidToken(inputText) switch
            {
                true => await TokenExists(currentInput.Details.Text.GetValueOrDefault()) switch
                {
                    true => await AuthenticateUserAsync(currentInput),
                    
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
    
    public States DetermineCurrentState(IReadOnlyCollection<TlgInput> tlgAgentInputHistory)
    {
        var lastTextSubmitted = tlgAgentInputHistory
            .LastOrDefault(i => i.InputType == TlgInputType.TextMessage);

        return lastTextSubmitted switch
        {
            null => Initial,
            _ => ReceivedTokenSubmissionAttempt
        };
    }

    private async Task<bool> TokenExists(string tokenAttempt) =>
        (await rolesRepo.GetAllAsync())
        .Any(role => role.Token == tokenAttempt);

    private async Task<List<OutputDto>> AuthenticateUserAsync(TlgInput tokenInputAttempt)
    {
        var inputText = tokenInputAttempt.Details.Text.GetValueOrThrow();
        var currentMode = tokenInputAttempt.TlgAgent.Mode;
        
        var outputs = new List<OutputDto>();
        
        var newTlgAgentRoleBindForCurrentMode = new TlgAgentRoleBind(
            (await rolesRepo.GetAllAsync()).First(r => r.Token == inputText),
            tokenInputAttempt.TlgAgent with { Mode = currentMode },
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var preExistingRoleBindings = 
            (await roleBindingsRepo.GetAllActiveAsync())
            .ToImmutableReadOnlyCollection();
        
        var preExistingActiveRoleBind = 
            FirstOrDefaultPreExistingActiveRoleBind(currentMode);

        if (preExistingActiveRoleBind != null)
        {
            await roleBindingsRepo.UpdateStatusAsync(preExistingActiveRoleBind, DbRecordStatus.Historic);
            
            outputs.Add(new OutputDto
            {
                Text = Ui("""
                          Warning: you were already authenticated with this token in another {0} chat. 
                          This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                          """, 
                    currentMode,
                    newTlgAgentRoleBindForCurrentMode.Role.RoleType,
                    newTlgAgentRoleBindForCurrentMode.Role.LiveEvent.Name)
            });
        }
        
        outputs.Add(new OutputDto
        {
            Text = Ui("{0}, you have successfully authenticated as a {1} at live-event {2}.", 
                newTlgAgentRoleBindForCurrentMode.Role.User.FirstName,
                newTlgAgentRoleBindForCurrentMode.Role.RoleType,
                newTlgAgentRoleBindForCurrentMode.Role.LiveEvent.Name)
        });

        outputs.Add(new OutputDto
        {
            Text = IInputProcessor.SeeValidBotCommandsInstruction
        });

        var tlgAgentRoleBindingsToAdd = new List<TlgAgentRoleBind> { newTlgAgentRoleBindForCurrentMode };
        
        var isInputInPrivateBotChat = 
            tokenInputAttempt.TlgAgent.ChatId == tokenInputAttempt.TlgAgent.UserId;

        if (isInputInPrivateBotChat)
        {
            AddTlgAgentRoleBindingsForOtherModes();
        }
        
        await roleBindingsRepo.AddAsync(tlgAgentRoleBindingsToAdd);
        
        return outputs;
        
        TlgAgentRoleBind? FirstOrDefaultPreExistingActiveRoleBind(InteractionMode mode) =>
        preExistingRoleBindings.FirstOrDefault(tarb => 
            tarb.Role.Token == inputText &&
            tarb.TlgAgent.Mode == mode);

        void AddTlgAgentRoleBindingsForOtherModes()
        {
            var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
            var otherModes = allModes.Except(new [] { currentMode });

            tlgAgentRoleBindingsToAdd.AddRange(
                from mode in otherModes 
                where FirstOrDefaultPreExistingActiveRoleBind(mode) == null
                select newTlgAgentRoleBindForCurrentMode with
                {
                    TlgAgent = newTlgAgentRoleBindForCurrentMode.TlgAgent with { Mode = mode },
                    ActivationDate = DateTime.UtcNow
                });
        }
    }
    
    [Flags]
    internal enum States
    {
        Initial = 1,
        ReceivedTokenSubmissionAttempt = 1<<1
    }
}