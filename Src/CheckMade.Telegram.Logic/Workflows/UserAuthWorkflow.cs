using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;

namespace CheckMade.Telegram.Logic.Workflows;

internal class UserAuthWorkflow : IWorkflow
{
    public static readonly UiString AuthenticateWithToken = Ui("🌀 Please enter your 'role token' to authenticate: ");
    
    public Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return Task.FromResult<Result<IReadOnlyList<OutputDto>>>(
            new List<OutputDto> { new() { Text = AuthenticateWithToken } });
    }
}