using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Utils.Generic.InputValidator;

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
    private IEnumerable<TlgClientPortModeRole> _preExistingPortRoles = new List<TlgClientPortModeRole>();

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
        
        return await DetermineCurrentStateAsync(tlgInput.UserId, tlgInput.ChatId) switch
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
    
    internal async Task<States> DetermineCurrentStateAsync(TlgUserId userId, TlgChatId chatId)
    {
        var lastUsedTlgClientPortRole = _preExistingPortRoles
            .Where(cpr =>
                cpr.ClientPort == new TlgClientPort(userId, chatId) &&
                cpr.DeactivationDate.IsSome)
            .MaxBy(cpr => cpr.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastUsedTlgClientPortRole != null
            ? lastUsedTlgClientPortRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        var allRelevantInputs = (await _inputRepo.GetAllAsync(userId))
            .Where(i => i.Details.TlgDate > dateOfLastDeactivationForCutOff)
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
        var outputs = new List<OutputDto>();
        
        var newPortRole = new TlgClientPortModeRole(
            _preExistingRoles.First(r => r.Token == inputText),
            new TlgClientPort(tokenInputAttempt.UserId, tokenInputAttempt.ChatId),
            DateTime.Now,
            Option<DateTime>.None());
        
        var preExistingActivePortRole = _preExistingPortRoles.FirstOrDefault(cpr => 
            cpr.Role.Token == inputText && 
            cpr.Status == DbRecordStatus.Active);

        if (preExistingActivePortRole != null)
        {
            await _portRoleRepo.UpdateStatusAsync(preExistingActivePortRole, DbRecordStatus.Historic);
            
            outputs.Add(new OutputDto
            {
                Text = Ui("""
                          Warning: you were already authenticated with this token in another chat. 
                          This will be the new chat where you receive messages in your role {0} at {1}. 
                          """, 
                    newPortRole.Role.RoleType,
                    "Placeholder LiveEvent")
            });
        }
        
        outputs.Add(new OutputDto()
        {
            Text = Ui("{0}, you have successfully authenticated as a {1} at live-event {2}.",
                "Placeholder Name",
                newPortRole.Role.RoleType,
                "Placeholder LiveEvent")
        });

        // ToDo: add Welcome / checkOut BotCommands message HERE!!? The same one as with /start

        await _portRoleRepo.AddAsync(newPortRole);
        
        return outputs;
    }
    
    [Flags]
    internal enum States
    {
        ReadyToReceiveToken = 1,
        ReceivedTokenSubmissionAttempt = 1<<1,
    }
}