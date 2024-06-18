namespace CheckMade.Common.Model.Core;

public static class DomainCategories
{
    public enum RoleType
    {
        SanitaryOps_Admin = 10001,
        SanitaryOps_Inspector = 10002,
        SanitaryOps_Engineer = 10003,
        SanitaryOps_CleanLead = 10004,
        SanitaryOps_Observer = 10005,
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