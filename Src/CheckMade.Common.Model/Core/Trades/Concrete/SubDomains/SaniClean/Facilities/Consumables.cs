using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Facilities;

public record Consumables : ITradeFacility<SaniCleanTrade>
{
    public IReadOnlyCollection<Item> AffectedItems { get; init; } = [];
    
    public enum Item
    {
        ToiletPaper,
        PaperTowels,
        Soap,
    }
}