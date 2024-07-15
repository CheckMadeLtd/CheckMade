using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public interface IWorkflowState
{
    IReadOnlyCollection<OutputDto> MyPrompt();
    Task<Result<WorkflowResponse>> ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync();
}