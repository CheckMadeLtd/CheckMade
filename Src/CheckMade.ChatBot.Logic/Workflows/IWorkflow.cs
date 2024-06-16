using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflow
{
    Task<Result<IReadOnlyList<OutputDto>>> GetNextOutputAsync(TlgInput tlgInput);
}