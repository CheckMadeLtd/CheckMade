using System.Text;
using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Common.LiveEvents.SphereOfActionDetails;
using CheckMade.Core.Model.Common.Submissions;
using CheckMade.Core.Model.Common.Submissions.SubmissionTypes;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Core.Model.Bot.DTOs.Inputs;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Bot.Workflows.Ops.NewSubmission.NewSubmissionUtils;

namespace CheckMade.Bot.Workflows.ModelFactories;

public sealed record SubmissionFactory<T>(
    ILiveEventsRepository LiveEventsRepo,
    IRolesRepository RolesRepo,
    IDomainGlossary Glossary) 
    : ISubmissionFactory<T> where T : ITrade, new()
{
    public async Task<ISubmission> CreateAsync(IReadOnlyCollection<Input> inputs)
    {
        var currentTrade = new T();
        var role = (await RolesRepo.GetAsync(
            inputs.Last().OriginatorRole.GetValueOrThrow()))!;
        var allSpheres = await
            GetAllTradeSpecificSpheresAsync(
                new T(),
                inputs.Last().LiveEventContext.GetValueOrThrow(),
                LiveEventsRepo);
        
        var lastSelectedSubmissionTypeName =
            GetLastSubmissionType(inputs)
                .Name
                .GetTypeNameWithoutGenericParamSuffix();

        ISubmission submission = lastSelectedSubmissionTypeName switch
        {
            nameof(GeneralIssue<T>) =>
                new GeneralIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
                    Glossary),

            nameof(CleaningIssue<T>) =>
                new CleaningIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Facility: GetLastSelectedFacility(),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
                    Glossary),

            nameof(ConsumablesIssue<T>) =>
                new ConsumablesIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    AffectedItems: GetSelectedConsumablesItems(),
                    ReportedBy: role,
                    Glossary),

            nameof(StaffIssue<T>) =>
                new StaffIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
                    Glossary),

            nameof(TechnicalIssue<T>) =>
                new TechnicalIssue<T>(
                    Id: GetGuid(),
                    CreationDate: DateTimeOffset.UtcNow,
                    Sphere: GetLastSelectedSphere<T>(inputs, allSpheres),
                    Facility: GetLastSelectedFacility(),
                    Evidence: GetSubmittedEvidence(),
                    ReportedBy: role,
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
                    Glossary),

            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(lastSelectedSubmissionTypeName)} for {nameof(ITrade)} " +
                $"'{currentTrade.GetType().Name}': '{lastSelectedSubmissionTypeName}'")
        };

        return submission;

        Guid GetGuid()
        {
            var uniqueGuids =
                inputs
                    .Where(static i => i.WorkflowGuid.IsSome)
                    .Select(static i => i.WorkflowGuid.GetValueOrThrow())
                    .Distinct()
                    .ToList();

            return uniqueGuids.Count switch
            {
                0 => throw new InvalidOperationException("No Guid found in provided inputs, can't constitute submission."),
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

        SubmissionEvidence GetSubmittedEvidence()
        {
            var combinedDescriptionEvidence = new StringBuilder();

            var submittedDescriptions = 
                inputs
                    .Where(i =>
                        i.InputType == InputType.TextMessage &&
                        i.ResultantState.IsSome &&
                        i.ResultantState.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewSubmissionEvidenceEntry<T>)))
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
                        i.InputType == InputType.AttachmentMessage &&
                        i.ResultantState.IsSome &&
                        i.ResultantState.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewSubmissionEvidenceEntry<T>)))
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
                    i.InputType == InputType.CallbackQuery &&
                    i.Details.DomainTerm.IsSome &&
                    Glossary.GetAll(typeof(AssessmentRating)).Contains(i.Details.DomainTerm.GetValueOrThrow()))
                .Details.DomainTerm.GetValueOrThrow()
                .EnumValue!;
    }
}