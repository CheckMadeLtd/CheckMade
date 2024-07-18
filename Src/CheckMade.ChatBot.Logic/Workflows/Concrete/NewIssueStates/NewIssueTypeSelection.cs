using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueTypeSelection : IWorkflowState; 

internal record NewIssueTypeSelection<T>(IDomainGlossary Glossary) : INewIssueTypeSelection 
    where T : ITrade
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync()
    {
        return 
            Task.FromResult<IReadOnlyCollection<OutputDto>>(new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please select the type of issue:"),
                    DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(ITradeIssue<T>)))
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
        {
            return 
                    new WorkflowResponse(
                        new OutputDto { Text = Ui("Please answer only using the buttons above.") },
                        GetType(), Glossary);
        }

        return currentInput.Details.DomainTerm.GetValueOrThrow().TypeValue!.Name switch
        {
            nameof(CleanlinessIssue) => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueCleanlinessFacilitySelection(Glossary))
        };
    }
}