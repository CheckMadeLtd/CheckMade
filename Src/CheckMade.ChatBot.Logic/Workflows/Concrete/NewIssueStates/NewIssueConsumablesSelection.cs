using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueConsumablesSelection : IWorkflowState;

internal record NewIssueConsumablesSelection(IDomainGlossary Glossary) : INewIssueConsumablesSelection
{
    public Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(Option<int> editMessageId)
    {
        return Task.FromResult<IReadOnlyCollection<OutputDto>>(
            new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Choose affected consumables:"),
                    DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                        Glossary.GetAll(typeof(Consumables.Item))),
                    ControlPromptsSelection = ControlPrompts.Save | ControlPrompts.Back | ControlPrompts.Cancel,
                    EditReplyMarkupOfMessageId = editMessageId
                }
            });
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return Task.FromResult<Result<WorkflowResponse>>(WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this));

        throw new NotImplementedException();

        // if (currentInput.Details.DomainTerm.IsSome)
        // {
        //     return ToggleConsumable(currentInput.Details.DomainTerm.GetValueOrThrow());
        // }
        //
        // WorkflowResponse ToggleConsumable(DomainTerm selectedConsumable)
        // {
        //     
        // }
    }
}