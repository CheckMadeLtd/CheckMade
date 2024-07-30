using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;

internal interface INewIssueSubmissionConfirmation<T> : IWorkflowState;

internal sealed record NewIssueSubmissionConfirmation<T>(
    IDomainGlossary Glossary,
    IGeneralWorkflowUtils GeneralWorkflowUtils,
    IRolesRepository RoleRepo,
    ITlgAgentRoleBindingsRepository RoleBindingsRepo,
    IIssueFactory<T> Factory) 
    : INewIssueSubmissionConfirmation<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetPromptAsync(
        TlgInput currentInput, Option<int> editMessageId)
    {
        List<OutputDto> outputs =
        [
            new()
            {
                Text = Ui("✅ Submission succeeded!")
            }
        ];

        var interactiveHistory =
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);
        var notificationOutput = 
            await GetNotificationOutput(currentInput, interactiveHistory);
        
        outputs.AddRange(
            (await GetNotificationRecipients(interactiveHistory))
            .Select(recipient => 
                new OutputDto
                {
                    Text = notificationOutput, 
                    LogicalPort = recipient
                }));

        return outputs;
    }

    public Task<Result<WorkflowResponse>> GetWorkflowResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }

    private async Task<UiString> GetNotificationOutput(
        TlgInput currentInput, IReadOnlyCollection<TlgInput> interactiveHistory)
    {
        var lastGuid = interactiveHistory
            .Select(i => i.EntityGuid)
            .Last(g => g.IsSome)
            .GetValueOrThrow();
        
        var historyWithLastGuidAppliedToCurrentInput = 
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(
                currentInput with
                {
                    EntityGuid = lastGuid
                });
        
        var summary = 
            (await Factory.CreateAsync(historyWithLastGuidAppliedToCurrentInput))
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
    
    private async Task<IReadOnlyCollection<LogicalPort>> GetNotificationRecipients(
        IReadOnlyCollection<TlgInput> interactiveHistory)
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

        var recipients = new List<LogicalPort>(
            allAdminAndObservers.Concat(allRelevantSpecialist)
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

