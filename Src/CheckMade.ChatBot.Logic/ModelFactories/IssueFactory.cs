using System.Text;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.Issues.Concrete;
using CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
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
        var role = (await RolesRepo.GetAsync(
            inputs.Last().OriginatorRole.GetValueOrThrow()))!;
        var allSpheres = await
            GetAllTradeSpecificSpheresAsync(
                new T(),
                inputs.Last().LiveEventContext.GetValueOrThrow(),
                LiveEventsRepo);
        
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
            
            nameof(Assessment<T>) =>
                new Assessment<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Facility: GetLastSelectedFacility(),
                    Rating: GetAssessmentRating(),
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
                    .Where(static i => i.EntityGuid.IsSome)
                    .Select(static i => i.EntityGuid.GetValueOrThrow())
                    .Distinct()
                    .ToList();

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
                inputs.LastOrDefault(static i =>
                        i.Details.DomainTerm.IsSome &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IFacility)))?
                    .Details.DomainTerm.GetValueOrThrow()
                    .TypeValue!;

            return (IFacility)Activator.CreateInstance(lastFacilityType)!;
        }

        IssueEvidence GetSubmittedEvidence()
        {
            var combinedDescriptionEvidence = new StringBuilder();

            var submittedDescriptions = 
                inputs
                    .Where(i =>
                        i.InputType == TlgInputType.TextMessage &&
                        i.ResultantState.IsSome &&
                        i.ResultantState.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewIssueEvidenceEntry<T>)))
                    .Select(static i => i.Details.Text.GetValueOrThrow())
                    .ToArray();

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
                        i.ResultantState.IsSome &&
                        i.ResultantState.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewIssueEvidenceEntry<T>)))
                    .ToArray();

            List<AttachmentDetails> attachments = [];
            
            attachments
                .AddRange(submittedAttachments
                    .Select(static attachment => 
                        new AttachmentDetails(
                            attachment.Details.AttachmentInternalUri.GetValueOrThrow(),
                            attachment.Details.AttachmentType.GetValueOrThrow(),
                            attachment.Details.Text)));

            var combinedDescription = combinedDescriptionEvidence.ToString();
            
            return new IssueEvidence
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
                    i.ResultantState.IsSome &&
                    i.ResultantState.GetValueOrThrow().InStateId == 
                    Glossary.GetId(typeof(INewIssueSubmissionSucceeded<T>)));

            return isSubmitted
                ? IssueStatus.Reported
                : IssueStatus.Drafting;
        }
        
        IReadOnlyCollection<ConsumablesItem> GetSelectedConsumablesItems()
        {
            return Glossary.GetAll(typeof(ConsumablesItem))
                .Where(dt => dt.IsToggleOn(inputs))
                .Select(static dt => (ConsumablesItem)dt.EnumValue!)
                .ToArray();
        }

        AssessmentRating GetAssessmentRating() =>
            (AssessmentRating)inputs
                .Last(i =>
                    i.InputType == TlgInputType.CallbackQuery &&
                    i.Details.DomainTerm.IsSome &&
                    Glossary.GetAll(typeof(AssessmentRating)).Contains(i.Details.DomainTerm.GetValueOrThrow()))
                .Details.DomainTerm.GetValueOrThrow()
                .EnumValue!;
    }
}