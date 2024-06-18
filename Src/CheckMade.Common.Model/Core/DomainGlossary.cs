namespace CheckMade.Common.Model.Core;

public static class DomainGlossary
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
        Cleanliness = 100100,
        Technical = 100110,
        Consumable = 100120,
    }

    public enum SanitaryOpsConsumable
    {
        ToiletPaper = 100121,
        PaperTowels = 100122,
        Soap = 100123,
    }
    
    public enum SanitaryOpsFacility
    {
        Toilets = 100200,
        Showers = 100210,
        Staff = 100220,
        Other = 100290
    }
}