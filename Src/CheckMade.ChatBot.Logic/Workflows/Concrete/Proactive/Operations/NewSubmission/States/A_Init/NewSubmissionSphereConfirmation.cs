using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.NewIssueUtils;
// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.A_Init;

internal interface INewSubmissionSphereConfirmation<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewSubmissionSphereConfirmation<T>(
    ILiveEventsRepository LiveEventsRepo,    
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils WorkflowUtils,
    ITlgAgentRoleBindingsRepository RoleBindingsRepo,
    IStateMediator Mediator) 
    : INewSubmissionSphereConfirmation<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput,
        Option<TlgMessageId> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var nearSphere = await GetNearSphere();

        List<OutputDto> outputs = 
        [
            new()
            {
                Text = Ui("Please confirm: are you at '{0}'?", nearSphere.GetValueOrThrow().Name),
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

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        if (currentInput.InputType is not TlgInputType.CallbackQuery)
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
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                originalPrompt,
                                UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.Yes)]),
                            UpdateExistingOutputMessageId = currentInput.TlgMessageId
                        })),
            
            (long)ControlPrompts.No => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, 
                    Mediator.Next(typeof(INewSubmissionSphereSelection<T>)),
                    new PromptTransition(
                        new OutputDto
                        {
                            Text = UiConcatenate(
                                originalPrompt,
                                UiNoTranslate(" "),
                                controlPromptsGlossary.UiByCallbackId[new CallbackId((long)ControlPrompts.No)]),
                            UpdateExistingOutputMessageId = currentInput.TlgMessageId
                        })),
            
            _ => throw new ArgumentOutOfRangeException(nameof(currentInput.Details.ControlPromptEnumCode))
        };
    }
}