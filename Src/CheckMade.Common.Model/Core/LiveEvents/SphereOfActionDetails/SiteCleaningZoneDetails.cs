using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Core.LiveEvents.SphereOfActionDetails;

public record SiteCleaningZoneDetails(Option<Geo> GeoCoordinates) : ISphereOfActionDetails;