using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowState
{
    Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId);
    Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput);
    IDomainGlossary Glossary { get; }
}