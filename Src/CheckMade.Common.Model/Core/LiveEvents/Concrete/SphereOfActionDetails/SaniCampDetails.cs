namespace CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;

public sealed record SaniCampDetails(Option<Geo> GeoCoordinates) : ISphereOfActionDetails
{
    public bool HasFacilityGeneralMisc { get; init; } = true;

    public bool HasFacilitySaniConsumables { get; init; }
    public bool HasFacilityShowers { get; init; }
    public bool HasFacilityToilets { get; init; }
}