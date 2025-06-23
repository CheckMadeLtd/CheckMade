using System.Collections.Immutable;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using CheckMade.Core.Model.Bot.DTOs.Outputs;
using CheckMade.Core.Model.Common.Actors.RoleTypes;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.Model.Common.Submissions;
using CheckMade.Core.Model.Common.Submissions.SubmissionTypes;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Logic;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

namespace CheckMade.Services.Logic;

public sealed record StakeholderReporter<T>(
    IRolesRepository RoleRepo,
    ISubmissionFactory<T> SubmissionFactory) 
    : IStakeholderReporter<T> where T : ITrade, new()
{
    public async Task<IReadOnlyCollection<Output>> GetNewSubmissionNotificationsAsync(
        IReadOnlyCollection<Input> inputHistory, string currentSubmissionTypeName)
    {
        var newSubmission = 
            await SubmissionFactory.CreateAsync(inputHistory); 
        var completeSubmissionSummary = 
            newSubmission.GetSummary();
        var recipients = 
            await GetNotificationRecipientsAsync(inputHistory, newSubmission.Sphere, currentSubmissionTypeName);
        
        return 
            recipients
                .Select(recipient =>
                    new Output
                    {
                        Text = GetNotificationOutput(kvp =>
                            (recipient.Role.RoleType.GetSubmissionSummaryCategoriesForNotifications() & kvp.Key) != 0),
                        LogicalPort = recipient,
                        Attachments = GetAttachments()
                    })
                .ToImmutableArray();

        Option<IReadOnlyCollection<AttachmentDetails>> GetAttachments() =>
            newSubmission is ISubmissionWithEvidence issueWithEvidence 
                ? issueWithEvidence.Evidence.Attachments 
                : Option<IReadOnlyCollection<AttachmentDetails>>.None();
        
        UiString GetNotificationOutput(Func<KeyValuePair<SubmissionSummaryCategories, UiString>, bool> summaryFilter)
        {
            return 
                UiConcatenate(
                    Ui("New submission:"),
                    UiNewLines(1),
                    UiNoTranslate("- - - - - - - - - - - - - - - - - -"),
                    UiNewLines(1),
                    UiConcatenate(
                        completeSubmissionSummary.Where(summaryFilter)
                            .Select(static kvp => kvp.Value)
                            .ToArray()));
        }
    }

    private async Task<IReadOnlyCollection<LogicalPort>> GetNotificationRecipientsAsync(
        IReadOnlyCollection<Input> inputHistory,
        ISphereOfAction submissionSphere,
        string currentSubmissionTypeName)
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
        
        var allRelevantSpecialist = currentSubmissionTypeName switch
        {
            nameof(CleaningIssue<T>) or nameof(Assessment<T>) or nameof(ConsumablesIssue<T>) =>
                allRolesAtCurrentLiveEvent
                    .Where(static r => r.RoleType is TradeTeamLead<T>)
                    .ToArray(),
            
            nameof(TechnicalIssue<T>) =>
                allRolesAtCurrentLiveEvent
                    .Where(static r => r.RoleType is TradeEngineer<T>)
                    .ToArray(),
            
            _ => []
        };

        var filterOutSpecialists = allRelevantSpecialist
            .Where(r => r.RoleType is TradeTeamLead<SanitaryTrade> or TradeTeamLead<SiteCleanTrade>)
            .Where(static r => r.AssignedToSpheres.Count != 0)
            .Where(r => !r.AssignedToSpheres.Contains(submissionSphere))
            .ToArray();
        
        var currentRoleInfo = inputHistory.First().OriginatorRole.GetValueOrThrow();
        
        return new List<LogicalPort>(
                allAdminAndObservers.Concat(allRelevantSpecialist).Except(filterOutSpecialists)
                    .Where(r => !r.Equals(currentRoleInfo))
                    .Select(static r => new LogicalPort(
                        r,
                        InteractionMode.Notifications)))
            .ToArray();
    }
}
