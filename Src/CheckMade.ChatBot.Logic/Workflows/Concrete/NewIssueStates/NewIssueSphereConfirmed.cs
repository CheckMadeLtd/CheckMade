using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereConfirmed : IWorkflowState; 

internal class NewIssueSphereConfirmed : INewIssueSphereConfirmed
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