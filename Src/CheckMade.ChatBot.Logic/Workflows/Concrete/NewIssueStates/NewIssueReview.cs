using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueReview : IWorkflowState;

internal record NewIssueReview(IDomainGlossary Glossary) : INewIssueReview
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}