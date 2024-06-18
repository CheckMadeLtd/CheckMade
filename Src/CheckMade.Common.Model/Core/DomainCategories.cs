namespace CheckMade.Common.Model.Core;

public static class DomainCategories
{
    public enum RoleType
    {
        SanitaryOps_Admin = 1001,
        SanitaryOps_Inspector = 1002,
        SanitaryOps_Engineer = 1003,
        SanitaryOps_CleanLead = 1004,
        SanitaryOps_Observer = 1005,
    }
    
    public enum SanitaryOpsIssue
    {
    
    }
    
    public enum SanitaryOpsFacility
    {
        // IMPORTANT: 99,999 is the maximum allowed, to avoid clash with ControlPrompt Enum!
        // See also const DomainCategoryMaxThreshold
    
        // 1 Sanitary
        // 10 Sanitary Operations Trade 
    
        // SanitaryOps_IssueCleanliness = 10100,
        // SanitaryOps_IssueTechnical = 10110,
        // SanitaryOps_IssueConsumable = 10120,
    
        // SanitaryOps_ConsumableToiletPaper = 10121,
        // SanitaryOps_ConsumablePaperTowels = 10122,
        // SanitaryOps_ConsumableSoap = 10123,
    
        SanitaryOps_FacilityToilets = 10200,
        SanitaryOps_FacilityShowers = 10210,
        SanitaryOps_FacilityStaff = 10220,
        SanitaryOps_FacilityOther = 10290
    
    
        // 2 Trade: Venue Cleaning
    }
}