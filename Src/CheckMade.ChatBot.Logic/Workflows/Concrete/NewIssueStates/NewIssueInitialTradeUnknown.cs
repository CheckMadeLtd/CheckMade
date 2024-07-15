using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialTradeUnknown : IWorkflowState;

internal class NewIssueInitialTradeUnknown : INewIssueInitialTradeUnknown
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