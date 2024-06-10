using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using CheckMade.Common.Model.Telegram.UserInteraction;

namespace CheckMade.Telegram.Logic.Workflows;

internal class UserAuthWorkflow(ITlgInputRepository inputRepo) : IWorkflow
{
    public async Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return await DetermineCurrentStateAsync(tlgInput.UserId) switch
        {
            States.Virgin => new List<OutputDto> { new()
                {
                    Text = Ui("When you have your 'role token' ready (format 'ABC123'), click 'authenticate'."),
                    ControlPromptsSelection = new List<ControlPrompts> { ControlPrompts.Authenticate }
                } 
            },
            
            States.ReadyToEnterToken => new List<OutputDto> { new()
                {
                    Text = Ui("🌀 Please enter your role token: "),
                    ControlPromptsSelection = new List<ControlPrompts>
                    {
                        ControlPrompts.Submit,
                        ControlPrompts.Cancel
                    }
                }
            },
            
            States.TokenSubmitted => VerifyToken(tlgInput.Details.Text) switch
            {
                true => new List<OutputDto> { new ()
                    {
                        Text = Ui("You have successfully authenticated.")
                    }
                },
                false => Result<IReadOnlyList<OutputDto>>.FromError(Ui("Bad token!"))
            },
            
            _ => Result<IReadOnlyList<OutputDto>>.FromError(UiNoTranslate("Error"))
        };
    }

    private async Task<States> DetermineCurrentStateAsync(TlgUserId userId)
    {
        var allInputs = await inputRepo.GetAllAsync(userId);

        if (allInputs.FirstOrDefault(x => 
                x.Details.ControlPromptEnumCode == (long)ControlPrompts.Submit) != null)
        {
            return States.TokenSubmitted;
        }
        
        return States.Virgin;
    }

    private static bool VerifyToken(Option<string> enteredToken)
    {
        return false; // ToDo: implement actual rules
    }
    
    [Flags]
    private enum States
    {
        Virgin = 1,
        ReadyToEnterToken = 1<<1,
        TokenSubmitted = 1<<2,
        AuthenticationConfirmed = 1<<3
    }
}