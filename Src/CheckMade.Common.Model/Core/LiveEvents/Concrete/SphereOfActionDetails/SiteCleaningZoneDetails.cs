namespace CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;

public sealed record SiteCleaningZoneDetails(
        Option<Geo> GeoCoordinates,
        IReadOnlyCollection<DomainTerm> AvailableFacilities) 
    : ISphereOfActionDetails;