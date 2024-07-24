namespace CheckMade.Common.Model.Core.LiveEvents.Concrete.Facilities;

public record SaniConsumables : ITradeFacility
{
    public IReadOnlyCollection<Item> AffectedItems { get; init; } = [];
    
    public enum Item
    {
        ToiletPaper,
        PaperTowels,
        Soap,
    }
}