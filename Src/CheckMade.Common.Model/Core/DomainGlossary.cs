namespace CheckMade.Common.Model.Core;

public static class DomainGlossary
{
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
        Shower = 100210,
        Staff = 100220,
        Other = 100290
    }
}