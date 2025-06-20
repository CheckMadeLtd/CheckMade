using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.Model.Common.CrossCutting;
using CheckMade.Abstract.Domain.Model.Common.LiveEvents.SphereOfActionDetails;
using CheckMade.Abstract.Domain.Model.Common.Trades;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Common;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.C_Review;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using static CheckMade.Bot.Workflows.Ops.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;

public interface INewSubmissionConsumablesSelection<T> : IWorkflowStateNormal where T : ITrade, new();

public sealed record NewSubmissionConsumablesSelection<T>(
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    ILiveEventsRepository LiveEventsRepo) 
    : INewSubmissionConsumablesSelection<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        var interactiveHistory =
            await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
        var availableConsumables = 
            await GetAvailableConsumablesAsync<T>(interactiveHistory, currentInput, LiveEventsRepo);
        
        List<Output> outputs = 
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
                        .ToImmutableArray()),
                ControlPromptsSelection = ControlPrompts.Save | ControlPrompts.Back,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];
        
        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType is not InputType.CallbackQuery)
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
                    Mediator.Next(typeof(INewSubmissionReview<T>)),
                    new PromptTransition(
                        new Output
                        {
                            Text = UiConcatenate(
                                UiIndirect(currentInput.Details.Text.GetValueOrThrow()),
                                UiNoTranslate(" "),
                                await GetSelectedConsumablesAsync()),
                            UpdateExistingOutputMessageId = currentInput.MessageId
                        })),
            
            (long)ControlPrompts.Back => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionTypeSelection<T>)),
                    new PromptTransition(true)),
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentInput.Details.ControlPromptEnumCode)}: '{selectedControl}'")
        };

        async Task<UiString> GetSelectedConsumablesAsync()
        {
            var interactiveHistory =
                await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
            var availableConsumables =
                await GetAvailableConsumablesAsync<T>(interactiveHistory, currentInput, LiveEventsRepo);
            
            return Glossary.GetUi(
                availableConsumables
                    .Where(item =>
                        item.IsToggleOn(interactiveHistory))
                    .ToImmutableArray());
        }
    }
}