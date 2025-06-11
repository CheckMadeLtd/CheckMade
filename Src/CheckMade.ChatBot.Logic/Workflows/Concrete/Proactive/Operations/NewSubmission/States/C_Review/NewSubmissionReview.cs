using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.BusinessLogic;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Submissions.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.C_Review;

internal interface INewSubmissionReview<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewSubmissionReview<T>(
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    IIssueFactory<T> Factory,
    ITlgInputsRepository InputsRepo,
    IStakeholderReporter<T> Reporter) 
    : INewSubmissionReview<T> where T : ITrade, new()
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
        
        var issue = await Factory.CreateAsync(updatedHistoryWithGuid);
        var summary = issue.GetSummary();

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
                            .Where(static kvp =>
                                (SubmissionSummaryCategories.AllExceptOperationalInfo & kvp.Key) != 0)
                            .Select(static kvp => kvp.Value)
                            .ToArray())),
                ControlPromptsSelection = ControlPrompts.Submit | ControlPrompts.Cancel,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];

        return previousPromptFinalizer.Match(
            ppf => outputs.Prepend(ppf).ToImmutableArray(),
            () => outputs.ToImmutableArray());
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
                    Mediator.GetTerminator(typeof(INewIssueSubmissionSucceeded<T>)),
                    promptTransition: new PromptTransition(currentInput.TlgMessageId),
                    entityGuid: await GetLastGuidAsync()),
            
            (long)ControlPrompts.Cancel => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput,
                    Mediator.Next(typeof(INewSubmissionCancelConfirmation<T>)), 
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
                        ResultantState = new ResultantWorkflowState(
                            Glossary.GetId(typeof(NewIssueWorkflow)),
                            Glossary.GetId(typeof(INewIssueSubmissionSucceeded<T>)))
                    });
            
            var currentIssueTypeName =
                NewIssueUtils.GetLastIssueType(historyWithUpdatedCurrentInput)
                    .Name
                    .GetTypeNameWithoutGenericParamSuffix();

            return await Reporter.GetNewIssueNotificationsAsync(
                historyWithUpdatedCurrentInput, 
                currentIssueTypeName);
        }
        
        async Task<Guid> GetLastGuidAsync()
        {
            if (_lastGuidCache == Guid.Empty)
            {
                var interactiveHistory =
                    await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
            
                _lastGuidCache = interactiveHistory
                    .Select(static i => i.EntityGuid)
                    .Last(static g => g.IsSome)
                    .GetValueOrThrow();
            }

            return _lastGuidCache;
        }
    }
}