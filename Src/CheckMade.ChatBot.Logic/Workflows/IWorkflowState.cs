using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowState
{
    Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(TlgInput currentInput, Option<int> editMessageId);
    Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput);
    IDomainGlossary Glossary { get; }
}