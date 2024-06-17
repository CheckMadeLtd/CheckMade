using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.LangExt.InputValidator;

namespace CheckMade.ChatBot.Logic.Workflows;

using static UserAuthWorkflow.States;

internal class UserAuthWorkflow : IWorkflow
{
    private static readonly OutputDto EnterTokenPrompt = new()
    {
        Text = Ui("🌀 Please enter your role token (format '{0}'): ", GetTokenFormatExample())
    };

    private readonly ITlgInputRepository _inputRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly ITlgClientPortRoleRepository _portRoleRepo;

    private IEnumerable<Role> _preExistingRoles = new List<Role>();
    private IEnumerable<TlgClientPortRole> _preExistingPortRoles = new List<TlgClientPortRole>();

    private UserAuthWorkflow(ITlgInputRepository inputRepo,
        IRoleRepository roleRepo,
        ITlgClientPortRoleRepository portRoleRepo)
    {
        _inputRepo = inputRepo;
        _roleRepo = roleRepo;
        _portRoleRepo = portRoleRepo;
    }

    public static async Task<UserAuthWorkflow> CreateAsync(
        ITlgInputRepository inputRepo,
        IRoleRepository roleRepo,
        ITlgClientPortRoleRepository portRoleRepo)
    {
        var workflow = new UserAuthWorkflow(inputRepo, roleRepo, portRoleRepo);
        await workflow.InitAsync();
        return workflow;
    }

    private async Task InitAsync()
    {
        var getRolesTask = _roleRepo.GetAllAsync();
        var getPortRolesTask = _portRoleRepo.GetAllAsync();
        
        await Task.WhenAll(getRolesTask, getPortRolesTask);

        _preExistingRoles = getRolesTask.Result;
        _preExistingPortRoles = getPortRolesTask.Result;
    }
    
    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        var inputText = tlgInput.Details.Text.GetValueOrDefault();
        
        return await DetermineCurrentStateAsync(tlgInput.UserId, tlgInput.ChatId, tlgInput.InteractionMode) switch
        {
            ReadyToReceiveToken => new List<OutputDto> { EnterTokenPrompt },
            
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
                UiNoTranslate("Can't determine State in UserAuthWorkflow"))
        };
    }
    
    internal async Task<States> DetermineCurrentStateAsync(TlgUserId userId, TlgChatId chatId, InteractionMode mode)
    {
        var lastUsedTlgClientPortRole = _preExistingPortRoles
            .Where(cpr =>
                cpr.ClientPort == new TlgClientPort(userId, chatId, mode) &&
                cpr.DeactivationDate.IsSome)
            .MaxBy(cpr => cpr.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastUsedTlgClientPortRole != null
            ? lastUsedTlgClientPortRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        var allRelevantInputs = (await _inputRepo.GetAllAsync(userId))
            .Where(i => 
                i.Details.TlgDate.ToUniversalTime() > 
                dateOfLastDeactivationForCutOff.ToUniversalTime())
            .ToList().AsReadOnly();

        var lastTextSubmitted = allRelevantInputs
            .LastOrDefault(i => i.TlgInputType == TlgInputType.TextMessage);

        return lastTextSubmitted switch
        {
            null => ReadyToReceiveToken,
            _ => ReceivedTokenSubmissionAttempt,
        };
    }

    private async Task<bool> TokenExists(string tokenAttempt) =>
        (await _roleRepo.GetAllAsync()).Any(role => role.Token == tokenAttempt);

    // ToDo: replace Placeholders with actual data from DB/Setup
    private async Task<List<OutputDto>> AuthenticateUserAsync(TlgInput tokenInputAttempt)
    {
        var inputText = tokenInputAttempt.Details.Text.GetValueOrThrow();
        var originatingMode = tokenInputAttempt.InteractionMode;
        var outputs = new List<OutputDto>();
        
        var newPortRoleForOriginatingMode = new TlgClientPortRole(
            _preExistingRoles.First(r => r.Token == inputText),
            new TlgClientPort(tokenInputAttempt.UserId, tokenInputAttempt.ChatId, originatingMode),
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var preExistingActivePortRole = FirstOrDefaultPreExistingActivePortRoleMode(originatingMode);

        if (preExistingActivePortRole != null)
        {
            await _portRoleRepo.UpdateStatusAsync(preExistingActivePortRole, DbRecordStatus.Historic);
            
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
        
        var isInputTlgClientPortPrivateChat = tokenInputAttempt.ChatId == tokenInputAttempt.UserId;

        if (isInputTlgClientPortPrivateChat)
        {
            AddPortRolesForOtherNonOriginatingAndVirginModes();
        }
        
        await _portRoleRepo.AddAsync(portRolesToAdd);
        
        return outputs;
        
        TlgClientPortRole? FirstOrDefaultPreExistingActivePortRoleMode(InteractionMode mode) =>
        _preExistingPortRoles.FirstOrDefault(cpr => 
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
        ReadyToReceiveToken = 1,
        ReceivedTokenSubmissionAttempt = 1<<1,
    }
}