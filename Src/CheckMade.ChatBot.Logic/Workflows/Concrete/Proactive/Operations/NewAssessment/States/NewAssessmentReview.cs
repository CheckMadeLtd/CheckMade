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

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}