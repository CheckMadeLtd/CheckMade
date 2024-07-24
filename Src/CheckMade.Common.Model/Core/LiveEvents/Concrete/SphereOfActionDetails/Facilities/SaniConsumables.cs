namespace CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails.Facilities;

public sealed record SaniConsumables : IFacility
{
    public IReadOnlyCollection<Item> AffectedItems { get; init; } = [];
    
    public enum Item
    {
        ToiletPaper,
        PaperTowels,
        Soap,
    }
}