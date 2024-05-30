namespace CheckMade.Common.Model.Enums;

public enum DomainCategory
{
    // IMPORTANT: 99,999 is the maximum allowed, to avoid clash with ControlPrompt Enum!
    // See also const DomainCategoryThreshold
    
    // 1 Sanitary
    // 10 Sanitary Operations Trade 
    
    SanitaryOps_RoleAdmin = 10010,
    SanitaryOps_RoleInspector = 10020,
    SanitaryOps_RoleEngineer = 10030,
    SanitaryOps_RoleCleanLead = 10040,
    SanitaryOps_RoleObserver = 10090,
    
    SanitaryOps_IssueCleanliness = 10100,
    SanitaryOps_IssueTechnical = 10110,
    SanitaryOps_IssueConsumable = 10120,
    
    SanitaryOps_ConsumableToiletPaper = 10121,
    SanitaryOps_ConsumablePaperTowels = 10122,
    SanitaryOps_ConsumableSoap = 10123,
    
    SanitaryOps_FacilityToilets = 10200,
    SanitaryOps_FacilityShowers = 10210,
    SanitaryOps_FacilityStaff = 10220,
    SanitaryOps_FacilityOther = 10290
    
    
    // 2 Trade: Venue Cleaning
}