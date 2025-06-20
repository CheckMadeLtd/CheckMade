using CheckMade.Core.Model.Common.GIS;
using GeoCoordinatePortable;

namespace CheckMade.Core.Model.Utils;

public static class GeoUtils
{
    public static double MetersAwayFrom(this Geo fromPoint, Geo toPoint)
    {
        var coordinate1 = new GeoCoordinate(fromPoint.Latitude, fromPoint.Longitude);
        var coordinate2 = new GeoCoordinate(toPoint.Latitude, toPoint.Longitude);

        return coordinate1.GetDistanceTo(coordinate2);
    }
}