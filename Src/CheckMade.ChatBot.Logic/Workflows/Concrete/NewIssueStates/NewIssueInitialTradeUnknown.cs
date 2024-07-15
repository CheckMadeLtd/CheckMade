using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueInitialTradeUnknown : IWorkflowState;

internal class NewIssueInitialTradeUnknown(
        IDomainGlossary glossary) 
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
                        glossary.GetAll(typeof(ITrade))) 
            }
        };
    }

    public Task<Result<WorkflowResponse>> ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync()
    {
        throw new NotImplementedException();
    }
}