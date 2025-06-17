using CheckMade.Abstract.Domain.Data.Core.GIS;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Data.Core.LiveEvents.SphereOfActionDetails;

public sealed record SanitaryCampDetails : ISphereOfActionDetails
{
    public SanitaryCampDetails(Option<Geo> GeoCoordinates,
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