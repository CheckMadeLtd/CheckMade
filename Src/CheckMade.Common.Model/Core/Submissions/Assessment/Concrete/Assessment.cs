using System.Collections.Frozen;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Core.Submissions.Assessment.Concrete.AssessmentSummaryCategories;

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
        return new Dictionary<AssessmentSummaryCategories, UiString>
        {
            [CommonBasics] = AssessmentFormatters.FormatCommonBasics(this, Glossary),
            [OperationalInfo] = AssessmentFormatters.FormatOperationalInfo(this, Glossary),
            [FacilityInfo] = AssessmentFormatters.FormatFacilityInfo(this, Glossary),
            [EvidenceInfo] = AssessmentFormatters.FormatEvidenceInfo(this)
        }.ToFrozenDictionary();
    }
}