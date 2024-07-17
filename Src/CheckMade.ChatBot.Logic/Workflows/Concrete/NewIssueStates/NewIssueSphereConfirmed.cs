using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSphereConfirmed : IWorkflowState; 

internal record NewIssueSphereConfirmed : INewIssueSphereConfirmed
{
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Please select the type of issue:"),
                
                // ToDo: Try adding generic T back to ITradeIssue so it can represent a trade-specific supertype for this:
                // DomainTermSelection = glossary.GetAll()
            }
        };
    }

    public Task<Result<WorkflowResponse>> 
        ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}