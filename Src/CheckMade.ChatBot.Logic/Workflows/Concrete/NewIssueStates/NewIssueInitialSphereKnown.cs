using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialSphereKnown : IWorkflowState; 

internal class NewIssueInitialSphereKnown : INewIssueInitialSphereKnown
{
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync()
    {
        throw new NotImplementedException();
    }
}