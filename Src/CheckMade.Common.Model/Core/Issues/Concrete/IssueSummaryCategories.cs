namespace CheckMade.Common.Model.Core.Issues.Concrete;

[Flags]
public enum IssueSummaryCategories
{
    None = 1,
    CommonBasics = 1 << 1,
    OperationalInfo = 1 << 2,
    FacilityInfo = 1 << 3,
    EvidenceInfo = 1 << 4,
    IssueSpecificInfo = 1 << 5,
    
    AllExceptOperationalInfo = CommonBasics | FacilityInfo | EvidenceInfo | IssueSpecificInfo,
    All = CommonBasics | OperationalInfo | FacilityInfo | EvidenceInfo | IssueSpecificInfo
}