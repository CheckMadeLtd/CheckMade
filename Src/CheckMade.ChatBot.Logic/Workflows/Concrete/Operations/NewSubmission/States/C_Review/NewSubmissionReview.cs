using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core.Submissions;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Logic;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.C_Review;

internal interface INewSubmissionReview<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewSubmissionReview<T>(
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    ISubmissionFactory<T> Factory,
    ITlgInputsRepository InputsRepo,
    IStakeholderReporter<T> Reporter,
    ILastOutputMessageIdCache MsgIdCache) 
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
        
        var submission = await Factory.CreateAsync(updatedHistoryWithGuid);
        var summary = submission.GetSummary();

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
        if (currentInput.InputType is not InputType.CallbackQuery)
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
                    Mediator.GetTerminator(typeof(INewSubmissionSucceeded<T>)),
                    promptTransition: new PromptTransition(currentInput.TlgMessageId, MsgIdCache, currentInput.TlgAgent),
                    entityGuid: await GetLastGuidAsync()),
            
            (long)ControlPrompts.Cancel => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput,
                    Mediator.Next(typeof(INewSubmissionCancelConfirmation<T>)), 
                    new PromptTransition(currentInput.TlgMessageId, MsgIdCache, currentInput.TlgAgent)),
            
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
                            Glossary.GetId(typeof(NewSubmissionWorkflow)),
                            Glossary.GetId(typeof(INewSubmissionSucceeded<T>)))
                    });
            
            var currentSubmissionTypeName =
                NewSubmissionUtils.GetLastSubmissionType(historyWithUpdatedCurrentInput)
                    .Name
                    .GetTypeNameWithoutGenericParamSuffix();

            return await Reporter.GetNewSubmissionNotificationsAsync(
                historyWithUpdatedCurrentInput, 
                currentSubmissionTypeName);
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