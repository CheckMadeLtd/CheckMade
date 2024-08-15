using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.BusinessLogic;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewAssessment.States;

internal interface INewAssessmentReview : IWorkflowStateNormal;

internal sealed record NewAssessmentReview(
    IDomainGlossary Glossary, 
    IStateMediator Mediator,
    IGeneralWorkflowUtils WorkflowUtils,
    IAssessmentFactory Factory,
    ITlgInputsRepository InputsRepo,
    IStakeholderReporter<SanitaryTrade> Reporter) 
    : INewAssessmentReview
{
    private Guid _lastGuidCache = Guid.Empty;
    
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<TlgMessageId> inPlaceUpdateMessageId, 
        Option<OutputDto> previousPromptFinalizer)
    {
        var interactiveHistory =
            await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
        await InputsRepo
            .UpdateGuid(interactiveHistory, Guid.NewGuid());
        var updatedHistoryWithGuid = 
            await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);

        var assessment = await Factory.CreateAsync(updatedHistoryWithGuid);
        var summary = assessment.GetSummary();

        List<OutputDto> outputs =
        [
            new()
            {
                Text = UiConcatenate(
                    Ui("Please review all details before submitting."),
                    UiNewLines(1),
                    UiNoTranslate("- - - - - - - - - - - - - - - - - -"),
                    UiNewLines(1),
                    UiConcatenate(
                        summary
                            .Where(kvp =>
                                (AssessmentSummaryCategories.AllExceptOperationalInfo & kvp.Key) != 0)
                            .Select(kvp => kvp.Value)
                            .ToArray())),
                ControlPromptsSelection = ControlPrompts.Submit | ControlPrompts.Cancel,
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

        var selectedControl = 
            currentInput.Details.ControlPromptEnumCode.GetValueOrThrow();

        return selectedControl switch
        {
            (long)ControlPrompts.Submit =>
                WorkflowResponse.Create(
                    currentInput,
                    new OutputDto
                    {
                        Text = Ui("✅ Submission succeeded!")
                    },
                    await GetStakeholderNotificationsAsync(),
                    Mediator.GetTerminator(typeof(INewAssessmentSubmissionSucceeded)),
                    promptTransition: new PromptTransition(currentInput.TlgMessageId),
                    entityGuid: await GetLastGuidAsync()),
            
            (long)ControlPrompts.Cancel => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput,
                    Mediator.Next(typeof(INewAssessmentCancelled)), 
                    new PromptTransition(currentInput.TlgMessageId)),
            
            _ => throw new InvalidOperationException($"Unhandled choice of {nameof(ControlPrompts)}")
        };

        async Task<IReadOnlyCollection<OutputDto>> GetStakeholderNotificationsAsync()
        {
            var historyWithUpdatedCurrentInput = 
                await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(
                    currentInput with
                    {
                        EntityGuid = await GetLastGuidAsync(),
                        ResultantWorkflow = new ResultantWorkflowState(
                            Glossary.GetId(typeof(NewAssessmentWorkflow)),
                            Glossary.GetId(typeof(INewAssessmentSubmissionSucceeded)))
                    });
            
            return await Reporter.GetNewAssessmentNotificationsAsync(historyWithUpdatedCurrentInput);
        }
        
        async Task<Guid> GetLastGuidAsync()
        {
            if (_lastGuidCache == Guid.Empty)
            {
                var interactiveHistory =
                    await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
            
                _lastGuidCache = interactiveHistory
                    .Select(i => i.EntityGuid)
                    .Last(g => g.IsSome)
                    .GetValueOrThrow();
            }

            return _lastGuidCache;
        }
    }
}