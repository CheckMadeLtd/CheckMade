using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using CheckMade.Common.Model.Telegram.UserInteraction;
using static CheckMade.Common.Model.Telegram.UserInteraction.ControlPrompts;
using static CheckMade.Common.Utils.Generic.InputValidator;

namespace CheckMade.Telegram.Logic.Workflows;

using static UserAuthWorkflow.States;

internal class UserAuthWorkflow(
        ITlgInputRepository inputRepo,
        ITlgClientPortToRoleMapRepository portToRoleMapRepo) 
    : IWorkflow
{
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
            
            ReadyToEnterToken => new List<OutputDto> { new()
                {
                    Text = Ui("🌀 Please enter your role token: "),
                    ControlPromptsSelection = Submit | Cancel
                }
            },
            
            TokenSubmitted => IsValidToken(tlgInput.Details.Text.GetValueOrDefault()) switch
            {
                true => new List<OutputDto> { new ()
                    {
                        Text = Ui("You have successfully authenticated.")
                    }
                },
                false => Result<IReadOnlyList<OutputDto>>.FromError(
                    Ui("Bad token format! The correct format is: '{0}'", GetTokenFormatExample()))
            },
            
            _ => Result<IReadOnlyList<OutputDto>>.FromError(UiNoTranslate("Error"))
        };
    }

    internal async Task<States> DetermineCurrentStateAsync(TlgUserId userId, TlgChatId chatId)
    {
        var lastUsedTlgClientPortToRoleMapping = (await portToRoleMapRepo.GetAllAsync())
            .Where(map =>
                map.ClientPort == new TlgClientPort(userId, chatId) &&
                map.DeactivationDate.IsSome)
            .MaxBy(map => map.DeactivationDate.GetValueOrThrow());

        var cutOffDate = lastUsedTlgClientPortToRoleMapping != null
            ? lastUsedTlgClientPortToRoleMapping.DeactivationDate.GetValueOrThrow()
            : DateTime.MinValue;
        
        var allInputsSinceDeactivationOfLastMapping = (await inputRepo.GetAllAsync(userId))
            .Where(i => i.Details.TlgDate > cutOffDate)
            .ToList().AsReadOnly();

        var allControlPromptsRespondedTo = (ControlPrompts)allInputsSinceDeactivationOfLastMapping
            .Where(i => i.Details.ControlPromptEnumCode.IsSome)
            .Select(i => i.Details.ControlPromptEnumCode.GetValueOrThrow())
            .Aggregate((current, next) => current | next);
        
        return allControlPromptsRespondedTo switch
        {
            var prompts when prompts.HasFlag(Authenticate) && !prompts.HasFlag(Submit) => ReadyToEnterToken,
            var prompts when prompts.HasFlag(Submit) => TokenSubmitted,
            _ => Virgin
        };
    }

    [Flags]
    internal enum States
    {
        Virgin = 1,
        ReadyToEnterToken = 1<<1,
        TokenSubmitted = 1<<2,
        AuthenticationConfirmed = 1<<3
    }
}