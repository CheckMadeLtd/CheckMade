namespace CheckMade.Common.Model;

public enum DomainCategory
{
    // 1 Trade: Sanitary Operations
    
    SanitaryOpsAdmin = 10010,
    Inspector = 10020,
    Engineer = 10030,
    CleanLead = 10040,
    Observer = 10090,
    
    SanitaryProblemCleanliness = 10100,
    SanitaryProblemTechnical = 10110,
    SanitaryProblemConsumable = 10120,
    
    SanitaryConsumableToiletPaper = 10121,
    SanitaryConsumablePaperTowels = 10122,
    SanitaryConsumableSoap = 10123,
    
    SanitaryFacilityToilets = 10200,
    SanitaryFacilityShowers = 10210,
    SanitaryFacilityStaff = 10220,
    SanitaryFacilityOther = 10290
    
    
    // 2 Trade: Venue Cleaning
}