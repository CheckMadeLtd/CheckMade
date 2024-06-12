using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using static CheckMade.Common.Utils.Generic.InputValidator;

namespace CheckMade.Telegram.Logic.Workflows;

using static UserAuthWorkflow.States;

internal class UserAuthWorkflow(
        ITlgInputRepository inputRepo,
        IRoleRepository roleRepo,
        ITlgClientPortRoleRepository portRoleRepo) 
    : IWorkflow
{
    private readonly OutputDto _enterTokenPrompt = new()
    {
        Text = Ui("🌀 Please enter your role token (format '{0}'): ", GetTokenFormatExample())
    };
    
    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return await DetermineCurrentStateAsync(tlgInput.UserId, tlgInput.ChatId) switch
        {
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
        var lastUsedTlgClientPortRole = (await portRoleRepo.GetAllAsync())
            .Where(cpr =>
                cpr.ClientPort == new TlgClientPort(userId, chatId) &&
                cpr.DeactivationDate.IsSome)
            .MaxBy(cpr => cpr.DeactivationDate.GetValueOrThrow());

        var dateOfLastDeactivationForCutOff = lastUsedTlgClientPortRole != null
            ? lastUsedTlgClientPortRole.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        var allRelevantInputs = (await inputRepo.GetAllAsync(userId))
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

    private async Task<bool> TokenExists(string token) =>
        (await roleRepo.GetAllAsync()).Any(role => role.Token == token);
    
    [Flags]
    internal enum States
    {
        ReadyToReceiveToken = 1,
        ReceivedTokenSubmissionAttempt = 1<<1,
    }
}