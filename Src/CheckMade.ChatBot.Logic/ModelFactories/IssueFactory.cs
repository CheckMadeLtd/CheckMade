using System.Text;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.Submissions;
using CheckMade.Common.Model.Core.Submissions.Issues;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.ModelFactories;

internal sealed record IssueFactory<T>(
    ILiveEventsRepository LiveEventsRepo,
    IRolesRepository RolesRepo,
    IDomainGlossary Glossary) 
    : IIssueFactory<T> where T : ITrade, new()
{
    public async Task<IIssue> CreateAsync(IReadOnlyCollection<TlgInput> inputs)
    {
        var currentTrade = new T();
        var liveEvent = (await LiveEventsRepo.GetAsync(
            inputs.Last().LiveEventContext.GetValueOrThrow()))!;
        var role = (await RolesRepo.GetAsync(
            inputs.Last().OriginatorRole.GetValueOrThrow()))!;
        var allSpheres = 
            GetAllTradeSpecificSpheres(liveEvent, new T());
        
        var lastSelectedIssueTypeName =
            GetLastIssueType(inputs)
                .Name
                .GetTypeNameWithoutGenericParamSuffix();

        IIssue issue = lastSelectedIssueTypeName switch
        {
            nameof(GeneralIssue<T>) =>
                new GeneralIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
                    HandledBy: Option<Role>.None(),
                    Status: GetStatus(),
                    Glossary),

            nameof(CleaningIssue<T>) =>
                new CleaningIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Facility: GetLastSelectedFacility(),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
                    HandledBy: Option<Role>.None(),
                    Status: GetStatus(),
                    Glossary),

            nameof(ConsumablesIssue<T>) =>
                new ConsumablesIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    AffectedItems: GetSelectedConsumablesItems(),
                    ReportedBy: role,
                    HandledBy: Option<Role>.None(),
                    Status: GetStatus(),
                    Glossary),

            nameof(StaffIssue<T>) =>
                new StaffIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
                    HandledBy: Option<Role>.None(),
                    Status: GetStatus(),
                    Glossary),

            nameof(TechnicalIssue<T>) =>
                new TechnicalIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Facility: GetLastSelectedFacility(),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
                    HandledBy: Option<Role>.None(),
                    Status: GetStatus(),
                    Glossary),

            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(lastSelectedIssueTypeName)} for {nameof(ITrade)} " +
                $"'{currentTrade.GetType().Name}': '{lastSelectedIssueTypeName}'")
        };

        return issue;

        Guid GetGuid()
        {
            var uniqueGuids =
                inputs
                    .Where(i => i.EntityGuid.IsSome)
                    .Select(i => i.EntityGuid.GetValueOrThrow())
                    .Distinct()
                    .ToImmutableReadOnlyCollection();

            return uniqueGuids.Count switch
            {
                0 => throw new InvalidOperationException("No Guid found in provided inputs, can't constitute entity."),
                > 1 => throw new InvalidOperationException($"Found {uniqueGuids.Count} Guids, expected 1."),
                _ => uniqueGuids.First()
            };
        }
        
        IFacility GetLastSelectedFacility()
        {
            var lastFacilityType =
                inputs.LastOrDefault(i =>
                        i.Details.DomainTerm.IsSome &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IFacility)))?
                    .Details.DomainTerm.GetValueOrThrow()
                    .TypeValue!;

            try
            {
                return (IFacility)Activator.CreateInstance(lastFacilityType)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Every subtype of {nameof(IFacility)} requires a parameterless constructor." +
                    $"This allows for usage of '{nameof(Activator)}.{nameof(Activator.CreateInstance)}' " +
                    $"instead of using a switch expression switching on the given {nameof(lastFacilityType)}. " +
                    $"We thereby reduce maintenance when new subtypes are added", ex);
            }
        }

        SubmissionEvidence GetSubmittedEvidence()
        {
            var combinedDescriptionEvidence = new StringBuilder();

            var submittedDescriptions = 
                inputs
                    .Where(i =>
                        i.InputType == TlgInputType.TextMessage &&
                        i.ResultantWorkflow.IsSome &&
                        i.ResultantWorkflow.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewIssueEvidenceEntry<T>)))
                    .Select(i => i.Details.Text.GetValueOrThrow())
                    .ToImmutableReadOnlyCollection();

            var lastDescription = submittedDescriptions.LastOrDefault();
            
            foreach (var text in submittedDescriptions)
            {
                combinedDescriptionEvidence.Append($"> {text}");

                if (text != lastDescription)
                    combinedDescriptionEvidence.Append('\n');
            }

            var submittedAttachments =
                inputs
                    .Where(i =>
                        i.InputType == TlgInputType.AttachmentMessage &&
                        i.ResultantWorkflow.IsSome &&
                        i.ResultantWorkflow.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewIssueEvidenceEntry<T>)))
                    .ToImmutableReadOnlyCollection();

            List<AttachmentDetails> attachments = [];
            
            attachments
                .AddRange(submittedAttachments
                    .Select(attachment => 
                        new AttachmentDetails(
                            attachment.Details.AttachmentInternalUri.GetValueOrThrow(),
                            attachment.Details.AttachmentType.GetValueOrThrow(),
                            attachment.Details.Text)));

            var combinedDescription = combinedDescriptionEvidence.ToString();
            
            return new SubmissionEvidence
            {
                Description = !string.IsNullOrWhiteSpace(combinedDescription)
                    ? combinedDescription
                    : Option<string>.None(),
                Attachments = attachments.Count != 0 
                    ? attachments
                    : Option<IReadOnlyCollection<AttachmentDetails>>.None()
            };
        }

        IssueStatus GetStatus()
        {
            // ToDo: Once we go beyond these two,
            // refactor to look for the highest/latest status and when found, return early
            
            var isSubmitted = 
                inputs.Any(i =>
                    i.ResultantWorkflow.IsSome &&
                    i.ResultantWorkflow.GetValueOrThrow().InStateId == 
                    Glossary.GetId(typeof(INewIssueSubmissionSucceeded<T>)));

            return isSubmitted
                ? IssueStatus.Reported
                : IssueStatus.Drafting;
        }
        
        IReadOnlyCollection<ConsumablesItem> GetSelectedConsumablesItems()
        {
            return Glossary.GetAll(typeof(ConsumablesItem))
                .Where(dt => dt.IsToggleOn(inputs))
                .Select(dt => (ConsumablesItem)dt.EnumValue!)
                .ToImmutableReadOnlyCollection();
        }
    }
}