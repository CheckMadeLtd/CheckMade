using CheckMade.Common.DomainModel.Data.Core.GIS;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Data.Core.LiveEvents.SphereOfActionDetails;

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