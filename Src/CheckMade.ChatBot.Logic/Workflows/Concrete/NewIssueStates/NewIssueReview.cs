using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueReview : IWorkflowState;

internal record NewIssueReview(IDomainGlossary Glossary) : INewIssueReview
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId)
    {
        // ToDo: the method of creating the right issue instance from the history should be in NewIssueWorkflow.cs
        // it's used here and elsewhere e.g. when submitting
        // here we should then just get an ITradeIssue<T> and simply call GetSummary() on it, not caring what the T is. 
        
        // CleanlinessIssue issue = new CleanlinessIssue();
        
        // return Task.FromResult<IReadOnlyCollection<OutputDto>>(
        //     new List<OutputDto>
        //     {
        //         new()
        //         {
        //             Text = issue.GetSummary()
        //         }
        //     });
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}