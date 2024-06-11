using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using CheckMade.Common.Model.Telegram.UserInteraction;
using static CheckMade.Common.Model.Telegram.UserInteraction.ControlPrompts;
using static CheckMade.Common.Utils.Generic.InputValidator;

namespace CheckMade.Telegram.Logic.Workflows;

using static UserAuthWorkflow.States;

internal class UserAuthWorkflow(ITlgInputRepository inputRepo) : IWorkflow
{
    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return await DetermineCurrentStateAsync(tlgInput.UserId) switch
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

    internal async Task<States> DetermineCurrentStateAsync(TlgUserId userId)
    {
        // ToDo: this needs to become not all of the user's inputs ever, but only inputs relevant to this workflow
        // i.e. since last 'reset'? This is a general problem that needs solving for each workflow
        var allInputs = (await inputRepo.GetAllAsync(userId)).ToList().AsReadOnly();

        var controlPrompts = (ControlPrompts)allInputs
            .Where(i => i.Details.ControlPromptEnumCode.IsSome)
            .Select(i => i.Details.ControlPromptEnumCode.GetValueOrThrow())
            .Aggregate((current, next) => current | next);
        
        return controlPrompts switch
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