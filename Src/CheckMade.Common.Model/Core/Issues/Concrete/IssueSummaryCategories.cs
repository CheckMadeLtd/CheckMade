namespace CheckMade.Common.Model.Core.Issues.Concrete;

[Flags]
public enum IssueSummaryCategories
{
    CommonBasics = 1,
    OperationalInfo = 1 << 1,
    FacilityInfo = 1 << 2,
    EvidenceInfo = 1 << 3,
    IssueSpecificInfo = 1 << 4,
    
    AllExceptOperationalInfo = CommonBasics | FacilityInfo | EvidenceInfo | IssueSpecificInfo,
    All = CommonBasics | OperationalInfo | FacilityInfo | EvidenceInfo | IssueSpecificInfo
}