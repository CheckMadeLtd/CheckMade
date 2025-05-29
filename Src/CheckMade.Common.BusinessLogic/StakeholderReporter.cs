using System.Collections.Immutable;
using CheckMade.Common.Interfaces.BusinessLogic;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.Issues;
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
        var newIssue = 
            await IssueFactory.CreateAsync(inputHistory); 
        var completeIssueSummary = 
            newIssue.GetSummary();
        var recipients = 
            await GetNewIssueNotificationRecipientsAsync(inputHistory, currentIssueTypeName);
        
        return 
            recipients
                .Select(recipient =>
                    new OutputDto
                    {
                        Text = GetNotificationOutput(kvp =>
                            (recipient.Role.RoleType.GetIssueSummaryCategoriesForNotifications() & kvp.Key) != 0),
                        LogicalPort = recipient,
                        Attachments = GetAttachments()
                    })
                .ToImmutableArray();

        Option<IReadOnlyCollection<AttachmentDetails>> GetAttachments() =>
            newIssue is IIssueWithEvidence issueWithEvidence 
                ? issueWithEvidence.Evidence.Attachments 
                : Option<IReadOnlyCollection<AttachmentDetails>>.None();
        
        UiString GetNotificationOutput(Func<KeyValuePair<IssueSummaryCategories, UiString>, bool> summaryFilter)
        {
            return 
                UiConcatenate(
                    Ui("New issue submission:"),
                    UiNewLines(1),
                    UiNoTranslate("- - - - - - - - - - - - - - - - - -"),
                    UiNewLines(1),
                    UiConcatenate(
                        completeIssueSummary.Where(summaryFilter)
                            .Select(static kvp => kvp.Value)
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
                .Where(static r => r.RoleType is 
                    TradeAdmin<T> or 
                    TradeObserver<T> or 
                    LiveEventAdmin or
                    LiveEventObserver)
                .ToArray();
        
        var allRelevantSpecialist = currentIssueTypeName switch
        {
            nameof(CleaningIssue<T>) =>
                allRolesAtCurrentLiveEvent
                    .Where(static r => r.RoleType is TradeTeamLead<T>)
                    .ToArray(),
            
            nameof(TechnicalIssue<T>) =>
                allRolesAtCurrentLiveEvent
                    .Where(static r => r.RoleType is TradeEngineer<T>)
                    .ToArray(),
            
            _ => []
        };

        var currentRole = inputHistory.First().OriginatorRole.GetValueOrThrow();
        
        var recipients = new List<LogicalPort>(
            allAdminAndObservers.Concat(allRelevantSpecialist)
                .Where(r => !r.Equals(currentRole))
                .Select(static r => new LogicalPort(
                    r,
                    InteractionMode.Notifications)));

        var allActiveRoleBindings = 
            await RoleBindingsRepo.GetAllActiveAsync();
        
        return recipients
            .Where(lp => 
                allActiveRoleBindings.Select(static tarb => tarb.Role)
                    .Contains(lp.Role))
            .ToArray();
    }
}
