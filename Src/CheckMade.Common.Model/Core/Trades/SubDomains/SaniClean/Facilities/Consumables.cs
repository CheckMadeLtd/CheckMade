using CheckMade.Common.Model.Core.Trades.Types;

namespace CheckMade.Common.Model.Core.Trades.SubDomains.SaniClean.Facilities;

public record Consumables : ITradeFacility<TradeSaniClean>
{
    public IReadOnlyCollection<Item> AffectedItems { get; init; } = [];
    
    public enum Item
    {
        ToiletPaper,
        PaperTowels,
        Soap,
    }
}