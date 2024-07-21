using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueReview<T> : IWorkflowState where T : ITrade;

internal record NewIssueReview<T>(
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils,
        IStateMediator Mediator) 
    : INewIssueReview<T> where T : ITrade
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var issue = NewIssueWorkflow.ConstructIssueAsync(
            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput));
        
        return new List<OutputDto>
        {
            new()
            {
                Text = issue.GetSummary()
            }
        };
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}