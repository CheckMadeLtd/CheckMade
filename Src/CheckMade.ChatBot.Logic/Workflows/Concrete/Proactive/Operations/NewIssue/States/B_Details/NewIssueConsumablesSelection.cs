using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;

internal interface INewIssueConsumablesSelection<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewIssueConsumablesSelection<T>(
        IDomainGlossary Glossary,
        IGeneralWorkflowUtils WorkflowUtils,
        IStateMediator Mediator,
        ILiveEventsRepository LiveEventsRepo) 
    : INewIssueConsumablesSelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var interactiveHistory =
            await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
        var availableConsumables = 
            await GetAvailableConsumablesAsync(interactiveHistory, currentInput);
        
        List<OutputDto> outputs = 
        [
            new()
            {
                Text = Ui("Choose affected consumables:"),
                DomainTermSelection = Option<IReadOnlyCollection<DomainTerm>>.Some(
                    Glossary.GetAll(typeof(ConsumablesItem))
                        .Where(item => availableConsumables.Contains(item))
                        .Select(item => 
                            item.IsToggleOn(interactiveHistory) 
                                ? item with { Toggle = true } 
                                : item with { Toggle = false })
                        .ToImmutableReadOnlyCollection()),
                ControlPromptsSelection = ControlPrompts.Save | ControlPrompts.Back,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableReadOnlyCollection(),
            () => outputs.ToImmutableReadOnlyCollection());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        if (currentInput.Details.DomainTerm.IsSome)
            return await WorkflowResponse.CreateFromNextStateAsync(
                currentInput, 
                this,
                new PromptTransition(true));

        var selectedControl = 
            currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();
        
        return selectedControl switch
        {
            (long)ControlPrompts.Save =>
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueReview<T>)),
                    new PromptTransition(
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                                UiNoTranslate(" "),
                                await GetSelectedConsumablesAsync()),
                            UpdateExistingOutputMessageId = currentInput.TlgMessageId
                        })),
            
            (long)ControlPrompts.Back => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewIssueTypeSelection<T>)),
                    new PromptTransition(true)),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControl}'")
        };

        async Task<UiString> GetSelectedConsumablesAsync()
        {
            var interactiveHistory =
                await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
            var availableConsumables =
                await GetAvailableConsumablesAsync(interactiveHistory, currentInput);
            
            return Glossary.GetUi(
                availableConsumables
                    .Where(item =>
                        item.IsToggleOn(interactiveHistory))
                    .ToImmutableReadOnlyCollection());
        }
    }

    private async Task<IReadOnlyCollection<DomainTerm>> GetAvailableConsumablesAsync(
        IReadOnlyCollection<TlgInput> interactiveHistory,
        TlgInput currentInput)
    {
        var currentSphere = 
            GetLastSelectedSphere(interactiveHistory, 
                GetAllTradeSpecificSpheres(
                    (await LiveEventsRepo.GetAsync(currentInput.LiveEventContext.GetValueOrThrow()))!,
                    new T()));

        return currentSphere.Details.AvailableConsumables;
    }
}