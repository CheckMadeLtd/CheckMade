using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

internal interface IWorkflow
{
    bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory);
    Task<Result<(IReadOnlyCollection<OutputDto> Output, Option<Enum> NewState)>> GetResponseAsync(TlgInput currentInput);
}