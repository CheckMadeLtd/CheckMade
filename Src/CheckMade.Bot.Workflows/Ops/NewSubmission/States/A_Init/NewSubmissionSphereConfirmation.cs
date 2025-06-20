using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using CheckMade.Abstract.Domain.Model.Common.LiveEvents;
using CheckMade.Abstract.Domain.Model.Common.Trades;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Bot;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Common;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Bot.Workflows.Ops.NewSubmission.NewSubmissionUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.A_Init;

public interface INewSubmissionSphereConfirmation<T> : IWorkflowStateNormal where T : ITrade, new();

public sealed record NewSubmissionSphereConfirmation<T>(
    ILiveEventsRepository LiveEventsRepo,    
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils WorkflowUtils,
    IAgentRoleBindingsRepository RoleBindingsRepo,
    IStateMediator Mediator) 
    : INewSubmissionSphereConfirmation<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput,
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        var nearSphere = await GetNearSphere();

        List<Output> outputs = 
        [
            new()
            {
                Text = UiConcatenate(
                    Ui("Please confirm: are you at '{0}'?", nearSphere.GetValueOrThrow().Name),
                    UiIndirect(
                        nearSphere.GetValueOrThrow().Details.LocationName.IsSome
                            ? " - " + nearSphere.GetValueOrThrow().Details.LocationName.GetValueOrDefault()
                            : string.Empty)),
                ControlPromptsSelection = ControlPrompts.YesNo,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];

        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());

        async Task<Option<ISphereOfAction>> GetNearSphere()
        {
            var lastKnownLocation = 
                await LastKnownLocationAsync(currentInput, WorkflowUtils);

            return lastKnownLocation.IsSome
                ? await SphereNearCurrentUserAsync(
                    currentInput.LiveEventContext.GetValueOrThrow(),
                    LiveEventsRepo,
                    lastKnownLocation.GetValueOrThrow(), 
                    new T(),
                    currentInput,
                    RoleBindingsRepo)
                : Option<ISphereOfAction>.None();
        }
    }

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
    {
        if (currentInput.InputType is not InputType.CallbackQuery)
            return WorkflowResponse.CreateWarningUseInlineKeyboardButtons(this);

        var controlPromptsGlossary = new ControlPromptsGlossary();
        var originalPrompt = UiIndirect(currentInput.Details.Text.GetValueOrThrow());
        
        return currentInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
        {
            (long)ControlPrompts.Yes => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionTypeSelection<T>)),
                    new PromptTransition(
                        new Output
                        {
                            Text = UiConcatenate(
                                originalPrompt,
                                UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.Yes)]),
                            UpdateExistingOutputMessageId = currentInput.MessageId
                        })),
            
            (long)ControlPrompts.No => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionSphereSelection<T>)),
                    new PromptTransition(
                        new Output
                        {
                            Text = UiConcatenate(
                                originalPrompt,
                                UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.No)]),
                            UpdateExistingOutputMessageId = currentInput.MessageId
                        })),
            
            _ => throw new ArgumentOutOfRangeException(nameof(currentInput.Details.ControlPromptEnumCode))
        };
    }
}