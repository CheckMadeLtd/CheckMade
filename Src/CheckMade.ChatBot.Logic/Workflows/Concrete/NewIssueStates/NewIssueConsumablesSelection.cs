using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueConsumablesSelection : IWorkflowState;

internal record NewIssueConsumablesSelection(
        IDomainGlossary Glossary,
        IReadOnlyCollection<TlgInput> InteractiveHistory,
        ITrade Trade,
        ILogicUtils LogicUtils) 
    : INewIssueConsumablesSelection
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
                        Glossary.GetAll(typeof(Consumables.Item))
                            .Select(dt => 
                                dt.IsToggleOn(InteractiveHistory) 
                                    ? dt with { Toggle = true } 
                                    : dt with { Toggle = false })
                            .ToImmutableReadOnlyCollection()),
                    ControlPromptsSelection = ControlPrompts.Save | ControlPrompts.Back,
                    EditPreviousOutputMessageId = editMessageId
                }
            });
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
            return await WorkflowResponse.CreateAsync(
                this, currentInput.Details.TlgMessageId);

        var selectedControlPrompt = 
            currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();
        
        return selectedControlPrompt switch
        {
            (long)ControlPrompts.Save =>
                await WorkflowResponse.CreateAsync(new NewIssueReview(Glossary)),
            (long)ControlPrompts.Back => Trade switch
            {
                SaniCleanTrade => 
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SaniCleanTrade>(Glossary, LogicUtils),
                        currentInput.Details.TlgMessageId),
                
                SiteCleanTrade =>
                    await WorkflowResponse.CreateAsync(
                        new NewIssueTypeSelection<SiteCleanTrade>(Glossary, LogicUtils),
                        currentInput.Details.TlgMessageId),
                
                _ => throw new InvalidOperationException(
                    $"Unhandled {nameof(Trade)}: '{Trade.GetType().Name}'")    
            },
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControlPrompt}'")
        };
    }
}