using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;

namespace CheckMade.Common.Model.Core.Submissions.Assessment;

public interface IAssessment : ISubmission
{
    AssessmentRating Rating { get; }
    IFacility Facility { get; }
    SubmissionEvidence Evidence { get; }
    
    IReadOnlyDictionary<AssessmentSummaryCategories, UiString> GetSummary();
}