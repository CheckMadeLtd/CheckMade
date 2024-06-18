namespace CheckMade.Common.Model.Core;

public static class DomainCategories
{
    // ToDo: OUTDATED COMMENTS, review after big DomainCategory Refactoring
    // ToDo: Probably remove the numbering system that spans across Enum types! 
    
    // IMPORTANT: 99,999 is the maximum allowed, to avoid clash with ControlPrompt Enum!
    // See also const DomainCategoryMaxThreshold

    // 1 Sanitary
    // 10 Sanitary Operations Trade 
    
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
        Cleanliness = 10100,
        Technical = 10110,
        Consumable = 10120,
    }

    public enum SanitaryOpsConsumable
    {
        ToiletPaper = 10121,
        PaperTowels = 10122,
        Soap = 10123,
    }
    
    public enum SanitaryOpsFacility
    {
        Toilets = 10200,
        Showers = 10210,
        Staff = 10220,
        Other = 10290
    }
}