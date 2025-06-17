namespace CheckMade.Abstract.Domain.Data.Core.Submissions;

/// <summary>
/// The order of these categories here determines the order during output/formatting.
/// </summary>
[Flags]
public enum SubmissionSummaryCategories
{
    None = 1,
    OperationalInfo = 1 << 1,
    CommonBasics = 1 << 2,
    FacilityInfo = 1 << 3,
    SubmissionTypeSpecificInfo = 1 << 4,
    EvidenceInfo = 1 << 5,
    
    AllExceptOperationalInfo = CommonBasics | FacilityInfo | SubmissionTypeSpecificInfo | EvidenceInfo,
    All = OperationalInfo | CommonBasics | FacilityInfo | SubmissionTypeSpecificInfo | EvidenceInfo
}