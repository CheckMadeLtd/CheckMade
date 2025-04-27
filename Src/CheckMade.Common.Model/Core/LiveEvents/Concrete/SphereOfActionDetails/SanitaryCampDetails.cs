using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;

namespace CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;

public sealed record SanitaryCampDetails : ISphereOfActionDetails
{
    public SanitaryCampDetails(Option<Geo> GeoCoordinates,
        IReadOnlyCollection<DomainTerm> AvailableFacilities,
        IReadOnlyCollection<DomainTerm> AvailableConsumables)
    {
        this.GeoCoordinates = GeoCoordinates;

        AvailableFacilities.ValidateFacilityDomainTerms();        
        this.AvailableFacilities = AvailableFacilities;
        AvailableConsumables.ValidateConsumablesDomainTerms();
        this.AvailableConsumables = AvailableConsumables;
    }

    public Option<Geo> GeoCoordinates { get; }
    public IReadOnlyCollection<DomainTerm> AvailableFacilities { get; }
    public IReadOnlyCollection<DomainTerm> AvailableConsumables { get; }
}