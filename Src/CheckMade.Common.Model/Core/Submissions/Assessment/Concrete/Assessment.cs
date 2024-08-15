using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;

public record Assessment(
    Guid Id, 
    DateTimeOffset CreationDate, 
    ISphereOfAction Sphere, 
    Role ReportedBy,
    AssessmentRating Rating, 
    IFacility Facility, 
    SubmissionEvidence Evidence) 
    : IAssessment
{
    public IReadOnlyDictionary<AssessmentSummaryCategories, UiString> GetSummary()
    {
        throw new NotImplementedException();
    }
}