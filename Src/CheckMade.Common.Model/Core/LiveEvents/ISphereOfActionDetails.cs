namespace CheckMade.Common.Model.Core.LiveEvents;

public interface ISphereOfActionDetails
{
    Option<Geo> GeoCoordinates { get; }
    
    bool HasFacilityGeneralMisc { get; }
    bool HasFacilitySaniConsumables { get; }
    bool HasFacilityShowers { get; }
    bool HasFacilityToilets { get; }
}