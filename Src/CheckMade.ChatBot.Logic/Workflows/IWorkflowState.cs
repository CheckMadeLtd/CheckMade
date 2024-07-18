using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowState
{
    Task<IReadOnlyCollection<OutputDto>> MyPromptAsync();
    Task<Result<WorkflowResponse>> ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput);
    IDomainGlossary Glossary { get; }
}