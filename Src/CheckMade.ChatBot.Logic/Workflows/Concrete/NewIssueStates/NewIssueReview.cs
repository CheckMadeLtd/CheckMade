using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueReview<T> : IWorkflowState where T : ITrade, new();

internal sealed record NewIssueReview<T>(
        IDomainGlossary Glossary,
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator,
        IIssueFactory<T> Factory,
        ITlgInputsRepository InputsRepo,
        IRolesRepository RoleRepo,
        ITlgAgentRoleBindingsRepository RoleBindingsRepo) 
    : INewIssueReview<T> where T : ITrade, new()
{
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
        
            outputs.AddRange(
                (await GetNotificationRecipientsAsync())
                .Select(recipient => 
                    new OutputDto
                    {
                        Text = notificationOutput, 
                        LogicalPort = recipient
                    }));

            return outputs;
            
            async Task<UiString> GetNotificationOutputAsync()
            {
                var lastGuid = interactiveHistory
                    .Select(i => i.EntityGuid)
                    .Last(g => g.IsSome)
                    .GetValueOrThrow();
        
                var historyWithUpdatedCurrentInput = 
                    await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(
                        currentInput with
                        {
                            EntityGuid = lastGuid,
                            ResultantWorkflow = new ResultantWorkflowInfo(
                                Glossary.GetId(typeof(INewIssueWorkflow)),
                                Glossary.GetId(GetType().GetInterfaces()[0]))
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

            async Task<IReadOnlyCollection<LogicalPort>> GetNotificationRecipientsAsync()
            {
                var allRolesAtCurrentLiveEvent = 
                    (await RoleRepo.GetAllAsync())
                    .Where(r => r.AtLiveEvent.Equals(
                        interactiveHistory.Last().LiveEventContext.GetValueOrThrow()))
                    .ToArray(); 
        
                var allAdminAndObservers =
                    allRolesAtCurrentLiveEvent
                        .Where(r => r.RoleType is 
                            TradeAdmin<T> or 
                            TradeObserver<T> or 
                            LiveEventAdmin or
                            LiveEventObserver)
                        .ToArray();
        
                var currentIssueTypeName =
                    NewIssueUtils.GetLastIssueType(interactiveHistory)
                        .Name
                        .GetTypeNameWithoutGenericParamSuffix();

                var allRelevantSpecialist = currentIssueTypeName switch
                {
                    nameof(CleanlinessIssue<T>) =>
                        allRolesAtCurrentLiveEvent
                            .Where(r => r.RoleType is TradeTeamLead<T>)
                            .ToArray(),
            
                    nameof(TechnicalIssue<T>) =>
                        allRolesAtCurrentLiveEvent
                            .Where(r => r.RoleType is TradeEngineer<T>)
                            .ToArray(),
            
                    _ => []
                };

                var currentRole = interactiveHistory.First().OriginatorRole.GetValueOrThrow();
        
                var recipients = new List<LogicalPort>(
                    allAdminAndObservers.Concat(allRelevantSpecialist)
                        .Where(r => !r.Equals(currentRole))
                        .Select(r => new LogicalPort(
                            r,
                            InteractionMode.Notifications)));

                var allActiveRoleBindings = 
                    await RoleBindingsRepo.GetAllActiveAsync();
        
                return recipients
                    .Where(lp => 
                        allActiveRoleBindings.Select(tarb => tarb.Role)
                            .Contains(lp.Role))
                    .ToImmutableReadOnlyCollection();
            }
        }
        
        async Task<Guid> GetLastGuidAsync()
        {
            var interactiveHistory =
                await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
            
            return interactiveHistory
                .Select(i => i.EntityGuid)
                .Last(g => g.IsSome)
                .GetValueOrThrow();
        }
    }
}