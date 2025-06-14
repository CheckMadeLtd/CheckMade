using CheckMade.Common.Domain.Data.Core.GIS;
using GeoCoordinatePortable;

namespace CheckMade.Common.Domain.Utils;

public static class GeoUtils
{
    public static double MetersAwayFrom(this Geo fromPoint, Geo toPoint)
    {
        var coordinate1 = new GeoCoordinate(fromPoint.Latitude, fromPoint.Longitude);
        var coordinate2 = new GeoCoordinate(toPoint.Latitude, toPoint.Longitude);

        return coordinate1.GetDistanceTo(coordinate2);
    }
}