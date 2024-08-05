using CheckMade.Common.Interfaces.BusinessLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.BusinessLogic;

public sealed record StakeholderReporter(
        IRolesRepository RoleRepo,
        ITlgAgentRoleBindingsRepository RoleBindingsRepo) 
    : IStakeholderReporter
{
    public Task<IReadOnlyCollection<OutputDto>> GetNewIssueNotificationsAsync<T>(IReadOnlyCollection<TlgInput> inputHistory, string currentIssueTypeName) where T : ITrade, new()
    {
        // For every type of stakeholder, potentially different details and control outputs!
        
        // outputs.AddRange(
        //     (await Reporter.GetNewIssueNotificationRecipientsAsync<T>(
        //         interactiveHistory, currentIssueTypeName))
        //     .Select(recipient => 
        //         new OutputDto
        //         {
        //             Text = notificationOutput, 
        //             LogicalPort = recipient
        //         }));
        //
        
        // async Task<UiString> GetNotificationOutputAsync()
        // {
        //     var summary = 
        //         (await Factory.CreateAsync(historyWithUpdatedCurrentInput))
        //         .GetSummary();
        //         
        //     return 
        //         UiConcatenate(
        //             Ui("New issue submission:"),
        //             UiNewLines(1),
        //             UiNoTranslate("- - - - - -"),
        //             UiNewLines(1),
        //             UiConcatenate(
        //                 summary.Where(kvp =>
        //                         (IssueSummaryCategories.All & kvp.Key) != 0)
        //                     .Select(kvp => kvp.Value)
        //                     .ToArray()));
        // }

        throw new NotImplementedException();
    }

    private async Task<IReadOnlyCollection<LogicalPort>> GetNewIssueNotificationRecipientsAsync<T>(
        IReadOnlyCollection<TlgInput> inputHistory, 
        string currentIssueTypeName) where T : ITrade, new()
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
