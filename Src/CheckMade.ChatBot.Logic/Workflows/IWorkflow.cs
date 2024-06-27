using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflow
{
    bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory);
    Task<Result<IReadOnlyCollection<OutputDto>>> GetNextOutputAsync(TlgInput currentInput);
}