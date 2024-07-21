using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueConsumablesSelection<T> : IWorkflowState where T : ITrade;

internal record NewIssueConsumablesSelection<T>(
        IDomainGlossary Glossary,
        ILogicUtils LogicUtils,
        INewIssueReview<T> NewIssueReview,
        INewIssueTypeSelection<T> NewIssueTypeSelection) 
    : INewIssueConsumablesSelection<T> where T : ITrade
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        var interactiveHistory =
            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        
        return new List<OutputDto> 
        {
            new()
            {
                Text = Ui("Choose affected consumables:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary.GetAll(typeof(Consumables.Item))
                        .Select(dt => 
                            dt.IsToggleOn(interactiveHistory) 
                                ? dt with { Toggle = true } 
                                : dt with { Toggle = false })
                        .ToImmutableReadOnlyCollection()),
                ControlPromptsSelection = ControlPrompts.Save | ControlPrompts.Back,
                EditPreviousOutputMessageId = editMessageId
            }
        };
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
            return await WorkflowResponse.CreateAsync(
                currentInput, this, true);

        var selectedControlPrompt = 
            currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();
        
        return selectedControlPrompt switch
        {
            (long)ControlPrompts.Save =>
                await WorkflowResponse.CreateAsync(currentInput, NewIssueReview),
            
            (long)ControlPrompts.Back => 
                await WorkflowResponse.CreateAsync(currentInput, NewIssueTypeSelection, true),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
        };
    }
}