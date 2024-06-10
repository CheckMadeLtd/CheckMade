using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;

namespace CheckMade.Telegram.Logic.Workflows;

public interface IWorkflow
{
    Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput);
}