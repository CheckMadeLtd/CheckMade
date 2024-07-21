using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueReview<T> : IWorkflowState where T : ITrade;

internal record NewIssueReview<T>(
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils,
        TlgInput CurrentInput) 
    : INewIssueReview<T> where T : ITrade
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var issue = NewIssueWorkflow.ConstructIssue(
            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(CurrentInput));
        
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