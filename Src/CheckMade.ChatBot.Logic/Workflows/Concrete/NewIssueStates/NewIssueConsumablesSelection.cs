using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueConsumablesSelection : IWorkflowState;

internal record NewIssueConsumablesSelection(IDomainGlossary Glossary) : INewIssueConsumablesSelection
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync()
    {
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Choose affected consumables:"),
                    DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(Consumables.Item)))
                    
                }
            });
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return Task.FromResult<Result<WorkflowResponse>>(WorkflowResponse.CreateOnlyUseInlineKeyboardButtonResponse(this));

        throw new NotImplementedException();
    }
}