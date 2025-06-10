namespace CheckMade.Common.Model.Core.Issues.Concrete;

/// <summary>
/// The order of these categories here determines the order during output/formatting.
/// </summary>
[Flags]
public enum IssueSummaryCategories
{
    None = 1,
    OperationalInfo = 1 << 1,
    CommonBasics = 1 << 2,
    FacilityInfo = 1 << 3,
    IssueSpecificInfo = 1 << 4,
    EvidenceInfo = 1 << 5,
    
    AllExceptOperationalInfo = CommonBasics | FacilityInfo | EvidenceInfo | IssueSpecificInfo,
    All = CommonBasics | OperationalInfo | FacilityInfo | EvidenceInfo | IssueSpecificInfo
}