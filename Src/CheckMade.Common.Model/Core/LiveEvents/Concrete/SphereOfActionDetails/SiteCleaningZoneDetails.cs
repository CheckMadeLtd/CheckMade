namespace CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;

public sealed record SiteCleaningZoneDetails : ISphereOfActionDetails
{
    public SiteCleaningZoneDetails(Option<Geo> GeoCoordinates,
        IReadOnlyCollection<DomainTerm> AvailableFacilities)
    {
        this.GeoCoordinates = GeoCoordinates;
        
        AvailableFacilities.ValidateFacilityDomainTerms();
        this.AvailableFacilities = AvailableFacilities;
    }

    public Option<Geo> GeoCoordinates { get; init; }
    public IReadOnlyCollection<DomainTerm> AvailableFacilities { get; init; }
}