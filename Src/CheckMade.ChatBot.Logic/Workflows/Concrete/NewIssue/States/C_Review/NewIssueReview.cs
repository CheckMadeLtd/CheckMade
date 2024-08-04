using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssue.States.D_Terminators;
using CheckMade.Common.Interfaces.BusinessLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssue.States.C_Review;

internal interface INewIssueReview<T> : IWorkflowStateNormal where T : ITrade, new();

internal sealed record NewIssueReview<T>(
        IDomainGlossary Glossary,
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator,
        IIssueFactory<T> Factory,
        ITlgInputsRepository InputsRepo,
        IRolesRepository RoleRepo,
        ITlgAgentRoleBindingsRepository RoleBindingsRepo,
        IStakeholderReporter Reporter) 
    : INewIssueReview<T> where T : ITrade, new()
{
    private Guid _lastGuidCache = Guid.Empty;
    
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, 
        Option<int> inPlaceUpdateMessageId,
        Option<OutputDto> previousPromptFinalizer)
    {
        var interactiveHistory =
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        await InputsRepo
            .UpdateGuid(interactiveHistory, Guid.NewGuid());
        var updatedHistoryWithGuid = 
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        
        var issue = await Factory.CreateAsync(updatedHistoryWithGuid);
        var summary = issue.GetSummary();

        List<OutputDto> outputs =
        [
            new()
            {
                Text = UiConcatenate(
                    Ui("Please review all details before submitting."),
                    UiNewLines(1),
                    UiNoTranslate("- - - - - -"),
                    UiNewLines(1),
                    UiConcatenate(
                        summary
                            .Where(kvp =>
                                (IssueSummaryCategories.AllExceptOperationalInfo & kvp.Key) != 0)
                            .Select(kvp => kvp.Value)
                            .ToArray())),
                ControlPromptsSelection = ControlPrompts.Submit | ControlPrompts.Cancel,
                UpdateExistingOutputMessageId = inPlaceUpdateMessageId
            }
        ];

        return previousPromptFinalizer.Match(
            ppf =>
            {
                outputs.Add(ppf);
                return outputs;
            },
            () => outputs);
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
                    Mediator.Terminate(typeof(INewIssueSubmissionSucceeded<T>)),
                    promptTransition: new PromptTransition(currentInput.TlgMessageId),
                    entityGuid: await GetLastGuidAsync()),
            
            (long)ControlPrompts.Cancel => 
                await WorkflowResponse.CreateFromNextStateAsync(
                    currentInput,
                    Mediator.Next(typeof(INewIssueCancelConfirmation<T>)), 
                    new PromptTransition(currentInput.TlgMessageId)),
            
            _ => throw new InvalidOperationException($"Unhandled choice of {nameof(ControlPrompts)}")
        };

        async Task<IReadOnlyCollection<OutputDto>> GetStakeholderNotificationsAsync()
        {
            List<OutputDto> outputs = [];
            
            var interactiveHistory =
                await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
            var notificationOutput = 
                await GetNotificationOutputAsync();
            var currentIssueTypeName =
                NewIssueUtils.GetLastIssueType(interactiveHistory)
                    .Name
                    .GetTypeNameWithoutGenericParamSuffix();

            outputs.AddRange(
                (await Reporter.GetNewIssueNotificationRecipientsAsync<T>(
                    interactiveHistory, currentIssueTypeName))
                .Select(recipient => 
                    new OutputDto
                    {
                        Text = notificationOutput, 
                        LogicalPort = recipient
                    }));

            return outputs;
            
            async Task<UiString> GetNotificationOutputAsync()
            {
                var historyWithUpdatedCurrentInput = 
                    await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(
                        currentInput with
                        {
                            EntityGuid = await GetLastGuidAsync(),
                            ResultantWorkflow = new ResultantWorkflowInfo(
                                Glossary.GetId(typeof(INewIssueWorkflow)),
                                Glossary.GetId(typeof(INewIssueSubmissionSucceeded<T>)))
                        });
        
                var summary = 
                    (await Factory.CreateAsync(historyWithUpdatedCurrentInput))
                    .GetSummary();
        
                return 
                    UiConcatenate(
                        Ui("New issue submission:"),
                        UiNewLines(1),
                        UiNoTranslate("- - - - - -"),
                        UiNewLines(1),
                        UiConcatenate(
                            summary.Where(kvp =>
                                    (IssueSummaryCategories.All & kvp.Key) != 0)
                                .Select(kvp => kvp.Value)
                                .ToArray()));
            }
        }
        
        async Task<Guid> GetLastGuidAsync()
        {
            if (_lastGuidCache == Guid.Empty)
            {
                var interactiveHistory =
                    await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
            
                _lastGuidCache = interactiveHistory
                    .Select(i => i.EntityGuid)
                    .Last(g => g.IsSome)
                    .GetValueOrThrow();
            }

            return _lastGuidCache;
        }
    }
}