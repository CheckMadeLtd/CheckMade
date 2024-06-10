using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;

namespace CheckMade.Telegram.Logic.Workflows;

internal class UserAuthWorkflow : IWorkflow
{
    public Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput)
    {
        return Task.FromResult<Result<IReadOnlyList<OutputDto>>>(
            new List<OutputDto> { new() { Text = IInputProcessor.AuthenticateWithToken } });
    }
}