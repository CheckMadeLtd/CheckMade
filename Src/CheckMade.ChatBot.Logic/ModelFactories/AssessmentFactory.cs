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
using CheckMade.Common.Model.Core.Submissions.Assessment;
using CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;
using CheckMade.Common.Model.Core.Submissions.Issues;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete.IssueTypes;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.ModelFactories;

internal sealed record AssessmentFactory(
    ILiveEventsRepository LiveEventsRepo,
    IRolesRepository RolesRepo,
    IDomainGlossary Glossary) 
    : IAssessmentFactory
{
    public async Task<IAssessment> CreateAsync(IReadOnlyCollection<TlgInput> inputs)
    {
        var liveEvent = (await LiveEventsRepo.GetAsync(
            inputs.Last().LiveEventContext.GetValueOrThrow()))!;
        var role = (await RolesRepo.GetAsync(
            inputs.Last().OriginatorRole.GetValueOrThrow()))!;
        var allSpheres = 
            GetAllTradeSpecificSpheres(liveEvent, new SanitaryTrade());
        
        IAssessment assessment = new Assessment(
            Id: GetGuid(),
            CreationDate: DateTimeOffset.UtcNow,
            Sphere: GetLastSelectedSphere<SanitaryTrade>(inputs, allSpheres),
            ReportedBy: role,
            Rating: GetRating(),
            Facility: GetLastSelectedFacility(),
            Evidence: GetSubmittedEvidence(),
            Glossary);
        
        return assessment;

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
                        Glossary.GetId(typeof(INewAssessmentEvidenceEntry)))
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
                        Glossary.GetId(typeof(INewAssessmentEvidenceEntry)))
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

        AssessmentRating GetRating()
        {
            throw new NotImplementedException();
        }
    }
}