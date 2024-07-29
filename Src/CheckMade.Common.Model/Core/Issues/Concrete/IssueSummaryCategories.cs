namespace CheckMade.Common.Model.Core.Issues.Concrete;

[Flags]
public enum IssueSummaryCategories
{
    CommonBasics = 1,
    MetaInfo = 1 << 1,
    FacilityInfo = 1 << 2,
    EvidenceInfo = 1 << 3,
    IssueSpecificInfo = 1 << 4,
    
    AllExceptMetaInfo = CommonBasics | FacilityInfo | EvidenceInfo | IssueSpecificInfo
}