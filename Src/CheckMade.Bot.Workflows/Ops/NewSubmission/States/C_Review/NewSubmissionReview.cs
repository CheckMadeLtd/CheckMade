using System.Collections.Immutable;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Common.Submissions;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Logic;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.D_Terminators;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using General.Utils.FpExtensions.Monads;

// ReSharper disable UseCollectionExpression

namespace CheckMade.Bot.Workflows.Ops.NewSubmission.States.C_Review;

public interface INewSubmissionReview<T> : IWorkflowStateNormal where T : ITrade, new();

public sealed record NewSubmissionReview<T>(
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    ISubmissionFactory<T> Factory,
    IStakeholderReporter<T> Reporter,
    ILastOutputMessageIdCache MsgIdCache) 
    : INewSubmissionReview<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<Output>> GetPromptAsync(
        Input currentInput, 
        Option<MessageId> inPlaceUpdateMessageId,
        Option<Output> previousPromptFinalizer)
    {
        var interactiveHistory =
            await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);
        
        var submission = await Factory.CreateAsync(interactiveHistory);
        var summary = submission.GetSummary();

        List<Output> outputs =
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

    public async Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(Input currentInput)
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
                    new Output
                    {
                        Text = Ui("✅ Submission succeeded!")
                    },
                    await GetStakeholderNotificationsAsync(),
                    Mediator.GetTerminator(typeof(INewSubmissionSucceeded<T>)),
                    promptTransition: new PromptTransition(currentInput.MessageId, MsgIdCache, currentInput.Agent)),
            
            (long)ControlPrompts.Cancel => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput,
                    Mediator.Next(typeof(INewSubmissionCancelConfirmation<T>)), 
                    new PromptTransition(currentInput.MessageId, MsgIdCache, currentInput.Agent)),
            
            _ => throw new InvalidOperationException($"Unhandled choice of {nameof(ControlPrompts)}")
        };

        async Task<IReadOnlyCollection<Output>> GetStakeholderNotificationsAsync()
        {
            var historyWithUpdatedCurrentInput = 
                await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(
                    currentInput with
                    {
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
    }
}