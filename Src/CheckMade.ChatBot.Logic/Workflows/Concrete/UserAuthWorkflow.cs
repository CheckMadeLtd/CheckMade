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
    Task<UserAuthWorkflow.States> DetermineCurrentStateAsync(TlgClientPort clientPort);
}

internal class UserAuthWorkflow(
        IRoleRepository roleRepo,
        ITlgClientPortRoleRepository portRoleRepo,
        IWorkflowUtils workflowUtils)
    : IUserAuthWorkflow
{
    private static readonly OutputDto EnterTokenPrompt = new()
    {
        Text = Ui("🌀 Please enter your role token (format '{0}'): ", GetTokenFormatExample())
    };

    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        var inputText = tlgInput.Details.Text.GetValueOrDefault();
        
        return await DetermineCurrentStateAsync(tlgInput.ClientPort) switch
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
            
            _ => Result<IReadOnlyList<OutputDto>>.FromError(
                UiNoTranslate($"Can't determine State in {nameof(UserAuthWorkflow)}"))
        };
    }
    
    public async Task<States> DetermineCurrentStateAsync(TlgClientPort clientPort)
    {
        var allRelevantInputs = await workflowUtils.GetAllCurrentInputsAsync(clientPort);
        
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

    // ToDo: replace Placeholders with actual data from DB/Setup
    private async Task<List<OutputDto>> AuthenticateUserAsync(TlgInput tokenInputAttempt)
    {
        var inputText = tokenInputAttempt.Details.Text.GetValueOrThrow();
        var originatingMode = tokenInputAttempt.ClientPort.Mode;
        var preExistingPortRoles = await workflowUtils.GetAllClientPortRolesAsync();
        
        var outputs = new List<OutputDto>();
        
        var newPortRoleForOriginatingMode = new TlgClientPortRole(
            (await roleRepo.GetAllAsync()).First(r => r.Token == inputText),
            tokenInputAttempt.ClientPort with { Mode = originatingMode },
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var preExistingActivePortRole = FirstOrDefaultPreExistingActivePortRoleMode(originatingMode);

        if (preExistingActivePortRole != null)
        {
            await portRoleRepo.UpdateStatusAsync(preExistingActivePortRole, DbRecordStatus.Historic);
            
            outputs.Add(new OutputDto
            {
                Text = Ui("""
                          Warning: you were already authenticated with this token in another {0} chat. 
                          This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                          """, 
                    originatingMode,
                    newPortRoleForOriginatingMode.Role.RoleType,
                    "Placeholder LiveEvent")
            });
        }
        
        outputs.Add(new OutputDto
        {
            Text = Ui("""
                      {0}, welcome to the CheckMade ChatBot!
                      You have successfully authenticated as a {1} at live-event {2}.
                      """, 
                newPortRoleForOriginatingMode.Role.User.FirstName,
                newPortRoleForOriginatingMode.Role.RoleType,
                "Placeholder LiveEvent")
        });

        outputs.Add(new OutputDto
        {
            Text = IInputProcessor.SeeValidBotCommandsInstruction
        });

        var portRolesToAdd = new List<TlgClientPortRole> { newPortRoleForOriginatingMode };
        
        var isInputTlgClientPortPrivateChat = 
            tokenInputAttempt.ClientPort.ChatId == tokenInputAttempt.ClientPort.UserId;

        if (isInputTlgClientPortPrivateChat)
        {
            AddPortRolesForOtherNonOriginatingAndVirginModes();
        }
        
        await portRoleRepo.AddAsync(portRolesToAdd);
        
        return outputs;
        
        TlgClientPortRole? FirstOrDefaultPreExistingActivePortRoleMode(InteractionMode mode) =>
        preExistingPortRoles.FirstOrDefault(cpr => 
            cpr.Role.Token == inputText &&
            cpr.ClientPort.Mode == mode && 
            cpr.Status == DbRecordStatus.Active);

        void AddPortRolesForOtherNonOriginatingAndVirginModes()
        {
            var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
            var nonOriginatingModes = allModes.Except(new [] { originatingMode });

            portRolesToAdd.AddRange(
                from mode in nonOriginatingModes 
                where FirstOrDefaultPreExistingActivePortRoleMode(mode) == null
                select newPortRoleForOriginatingMode with
                {
                    ClientPort = newPortRoleForOriginatingMode.ClientPort with { Mode = mode },
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