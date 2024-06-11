using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using static CheckMade.Common.Model.Telegram.UserInteraction.ControlPrompts;
using static CheckMade.Common.Utils.Generic.InputValidator;

namespace CheckMade.Telegram.Logic.Workflows;

using static UserAuthWorkflow.States;

internal class UserAuthWorkflow(
        ITlgInputRepository inputRepo,
        IRoleRepository roleRepo,
        ITlgClientPortToRoleMapRepository portToRoleMapRepo) 
    : IWorkflow
{
    private readonly OutputDto _enterTokenPrompt = new()
    {
        Text = Ui("🌀 Please enter your role token: "),
        ControlPromptsSelection = Submit | Cancel
    };
    
    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return await DetermineCurrentStateAsync(tlgInput.UserId, tlgInput.ChatId) switch
        {
            Virgin => new List<OutputDto> { new()
                {
                    Text = Ui("When you have your 'role token' ready (format '{0}'), click 'authenticate'.",
                        GetTokenFormatExample()),
                    ControlPromptsSelection = Authenticate
                } 
            },
            
            ReadyToReceiveToken => new List<OutputDto> { _enterTokenPrompt },
            
            ReceivedTokenSubmissionAttempt => IsValidToken(tlgInput.Details.Text.GetValueOrDefault()) switch
            {
                true => await TokenExists(tlgInput.Details.Text.GetValueOrDefault()) switch
                {
                    true => new List<OutputDto> { new ()
                        {
                            Text = Ui("You have successfully authenticated.")
                        }
                    },
                    false => [ new OutputDto
                        {
                            Text = Ui("This is an unknown token. Try again...")
                        },
                        _enterTokenPrompt ]
                },
                false => [ new OutputDto
                    {
                        Text = Ui("Bad token format! The correct format is: '{0}'", GetTokenFormatExample())
                    },
                    _enterTokenPrompt ]
            },
            
            _ => Result<IReadOnlyList<OutputDto>>.FromError(
                UiNoTranslate("Can't determine State in UserAuthWorkflow"))
        };
    }
    
    internal async Task<States> DetermineCurrentStateAsync(TlgUserId userId, TlgChatId chatId)
    {
        var lastUsedTlgClientPortToRoleMapping = (await portToRoleMapRepo.GetAllAsync())
            .Where(map =>
                map.ClientPort == new TlgClientPort(userId, chatId) &&
                map.DeactivationDate.IsSome)
            .MaxBy(map => map.DeactivationDate.GetValueOrThrow());

        var dateOfLastMappingDeactivationForCutOff = lastUsedTlgClientPortToRoleMapping != null
            ? lastUsedTlgClientPortToRoleMapping.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        var allRelevantInputs = (await inputRepo.GetAllAsync(userId))
            .Where(i => i.Details.TlgDate > dateOfLastMappingDeactivationForCutOff)
            .ToList().AsReadOnly();

        var lastAuthenticatePrompt = allRelevantInputs
            .LastOrDefault(i => i.Details.ControlPromptEnumCode.GetValueOrDefault() == (long)Authenticate);
        
        var lastTextSubmitted = allRelevantInputs
            .LastOrDefault(i => i.TlgInputType == TlgInputType.TextMessage);

        return (lastAuthenticatePrompt, lastTextSubmitted) switch
        {
            (null, _) => Virgin,
            
            (_,null) => ReadyToReceiveToken,
            
            ({ } lastAuth, { } lastText) 
                when lastText.Details.TlgDate > lastAuth.Details.TlgDate => ReceivedTokenSubmissionAttempt,
            
            _ => throw new InvalidOperationException($"Undetermined current State in {GetType()}")
        };
    }

    private async Task<bool> TokenExists(string token) =>
        (await roleRepo.GetAllAsync()).Any(role => role.Token == token);
    
    [Flags]
    internal enum States
    {
        Virgin = 1,
        ReadyToReceiveToken = 1<<1,
        ReceivedTokenSubmissionAttempt = 1<<2,
    }
}