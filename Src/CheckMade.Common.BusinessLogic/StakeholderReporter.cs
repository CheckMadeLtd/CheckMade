using CheckMade.Common.Interfaces.BusinessLogic;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.BusinessLogic;

public sealed record StakeholderReporter<T>(
        IRolesRepository RoleRepo,
        ITlgAgentRoleBindingsRepository RoleBindingsRepo,
        IIssueFactory<T> IssueFactory) 
    : IStakeholderReporter<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<OutputDto>> GetNewIssueNotificationsAsync(
        IReadOnlyCollection<TlgInput> inputHistory, string currentIssueTypeName)
    {
        var completeIssueSummary = 
            (await IssueFactory.CreateAsync(inputHistory))
            .GetSummary();

        var summaryCategoriesByRoleType = 
            new Dictionary<IRoleType, IssueSummaryCategories>
            {
                [new LiveEventAdmin()] = IssueSummaryCategories.CommonBasics,
                [new LiveEventObserver()] = IssueSummaryCategories.CommonBasics,
                [new TradeAdmin<T>()] = IssueSummaryCategories.All,
                [new TradeEngineer<T>()] = IssueSummaryCategories.AllExceptOperationalInfo,
                [new TradeInspector<T>()] = IssueSummaryCategories.None,
                [new TradeObserver<T>()] = IssueSummaryCategories.All,
                [new TradeTeamLead<T>()] = IssueSummaryCategories.AllExceptOperationalInfo
            };
        
        var recipients = 
            await GetNewIssueNotificationRecipientsAsync(inputHistory, currentIssueTypeName);

        return 
            recipients
                .Select(recipient =>
                    new OutputDto
                    {
                        Text = GetNotificationOutput(kvp =>
                            (summaryCategoriesByRoleType[recipient.Role.RoleType] & kvp.Key) != 0),
                        LogicalPort = recipient
                    })
                .ToImmutableReadOnlyCollection();
        
        UiString GetNotificationOutput(Func<KeyValuePair<IssueSummaryCategories, UiString>, bool> summaryFilter)
        {
            return 
                UiConcatenate(
                    Ui("New issue submission:"),
                    UiNewLines(1),
                    UiNoTranslate("- - - - - -"),
                    UiNewLines(1),
                    UiConcatenate(
                        completeIssueSummary.Where(summaryFilter)
                            .Select(kvp => kvp.Value)
                            .ToArray()));
        }
    }

    private async Task<IReadOnlyCollection<LogicalPort>> GetNewIssueNotificationRecipientsAsync(
        IReadOnlyCollection<TlgInput> inputHistory, 
        string currentIssueTypeName)
    {
        var allRolesAtCurrentLiveEvent = 
            (await RoleRepo.GetAllAsync())
            .Where(r => r.AtLiveEvent.Equals(
                inputHistory.Last().LiveEventContext.GetValueOrThrow()))
            .ToArray(); 
        
        var allAdminAndObservers =
            allRolesAtCurrentLiveEvent
                .Where(r => r.RoleType is 
                    TradeAdmin<T> or 
                    TradeObserver<T> or 
                    LiveEventAdmin or
                    LiveEventObserver)
                .ToArray();
        
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

        var currentRole = inputHistory.First().OriginatorRole.GetValueOrThrow();
        
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
