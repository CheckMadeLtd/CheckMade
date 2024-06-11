using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
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

    private async Task<States> DetermineCurrentStateAsync(TlgUserId userId)
    {
        var allInputs = await inputRepo.GetAllAsync(userId);

        if (allInputs.FirstOrDefault(x => x.Details.ControlPromptEnumCode == (long)Submit) != null)
        {
            return TokenSubmitted;
        }
        
        return Virgin;
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