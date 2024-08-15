namespace CheckMade.Common.Model.Core.Submissions.Assessment.Concrete;

[Flags]
public enum AssessmentSummaryCategories
{
    None = 1,
    CommonBasics = 1 << 1,
    OperationalInfo = 1 << 2,
    FacilityInfo = 1 << 3,
    EvidenceInfo = 1 << 4,
    
    AllExceptOperationalInfo = CommonBasics | FacilityInfo | EvidenceInfo,
    All = CommonBasics | OperationalInfo | FacilityInfo | EvidenceInfo
}