namespace CheckMade.Common.Model.Enums;

public enum DomainCategory
{
    // 1 Sanitary
    // 10 Sanitary Operations Trade 
    
    SanitaryOpsRoleAdmin = 10010,
    SanitaryOpsRoleInspector = 10020,
    SanitaryOpsRoleEngineer = 10030,
    SanitaryOpsRoleCleanLead = 10040,
    SanitaryOpsRoleObserver = 10090,
    
    SanitaryOpsIssueCleanliness = 10100,
    SanitaryOpsIssueTechnical = 10110,
    SanitaryOpsIssueConsumable = 10120,
    
    SanitaryOpsConsumableToiletPaper = 10121,
    SanitaryOpsConsumablePaperTowels = 10122,
    SanitaryOpsConsumableSoap = 10123,
    
    SanitaryOpsFacilityToilets = 10200,
    SanitaryOpsFacilityShowers = 10210,
    SanitaryOpsFacilityStaff = 10220,
    SanitaryOpsFacilityOther = 10290
    
    
    // 2 Trade: Venue Cleaning
}