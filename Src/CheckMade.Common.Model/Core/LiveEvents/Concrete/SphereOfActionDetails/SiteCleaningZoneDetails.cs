using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;

public sealed record SiteCleaningZoneDetails : ISphereOfActionDetails
{
    public SiteCleaningZoneDetails(Option<Geo> GeoCoordinates,
        IReadOnlyCollection<DomainTerm> AvailableFacilities, 
        IReadOnlyCollection<DomainTerm> AvailableConsumables, 
        Option<string> locationName)
    {
        this.GeoCoordinates = GeoCoordinates;
        
        AvailableFacilities.ValidateFacilityDomainTerms();
        this.AvailableFacilities = AvailableFacilities;
        AvailableConsumables.ValidateConsumablesDomainTerms();
        this.AvailableConsumables = AvailableConsumables;
        
        LocationName = locationName;
    }

    public Option<Geo> GeoCoordinates { get; }
    public IReadOnlyCollection<DomainTerm> AvailableFacilities { get; }
    public IReadOnlyCollection<DomainTerm> AvailableConsumables { get; }
    public Option<string> LocationName { get; }
}