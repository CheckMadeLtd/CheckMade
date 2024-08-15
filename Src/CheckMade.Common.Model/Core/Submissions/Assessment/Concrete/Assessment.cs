using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;

public sealed record Assessment(
    Guid Id, 
    DateTimeOffset CreationDate, 
    ISphereOfAction Sphere, 
    Role ReportedBy,
    AssessmentRating Rating, 
    IFacility Facility, 
    Option<SubmissionEvidence> Evidence,
    IDomainGlossary Glossary) 
    : IAssessment
{
    public IReadOnlyDictionary<AssessmentSummaryCategories, UiString> GetSummary()
    {
        throw new NotImplementedException();
    }
}