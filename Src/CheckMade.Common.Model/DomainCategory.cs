namespace CheckMade.Common.Model;

public enum DomainCategory
{
    // 1 Trade: Sanitary Operations
    // 100: Roles
    SanitaryOpsAdmin = 10010,
    Inspector = 10020,
    Engineer = 10030,
    CleanLead = 10040,
    Observer = 10090,
    // 101: Issues
    ProblemTypeCleanliness = 10100,
    ProblemTypeTechnical = 10110,
    ProblemTypeConsumable = 10120,
    // 1012: Consumables
    ToiletPaper = 10121,
    PaperTowels = 10122,
    Soap = 10123,
    // 102: Facilities
    Toilets = 10200,
    Showers = 10210,
    Staff = 10220,
    OtherFacilities = 10290
    
    
    // 2 Trade: Venue Cleaning
}