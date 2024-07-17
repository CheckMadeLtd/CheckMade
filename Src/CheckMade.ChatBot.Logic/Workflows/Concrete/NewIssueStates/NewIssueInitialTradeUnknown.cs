using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialTradeUnknown : IWorkflowState;

internal record NewIssueInitialTradeUnknown(
        IDomainGlossary Glossary) 
    : INewIssueInitialTradeUnknown
{
    public IReadOnlyCollection<OutputDto> MyPrompt()
    {
        return new List<OutputDto>
        {
            new()
            {
                Text = Ui("Please select a Trade:"),
                
                DomainTermSelection = 
                    Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITrade))) 
            }
        };
    }

    public Task<Result<WorkflowResponse>> 
        ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}