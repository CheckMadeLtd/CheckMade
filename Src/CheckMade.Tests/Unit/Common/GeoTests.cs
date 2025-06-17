using CheckMade.Abstract.Domain.Data.Core.GIS;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Tests.Unit.Common;

public class GeoTests
{
    [Theory]
    [InlineData(90.000001, 0)]
    [InlineData(-90.000001, 0)]
    [InlineData(0, 180.000001)]
    [InlineData(0, -180.000001)]
    public void GeoConstructor_Throws_ForOutOfBoundGeoCoordinates(double latitudeRaw, double longitudeRaw)
    {
        var invalidGeoCreator = 
            () => new Geo(latitudeRaw, longitudeRaw, Option<double>.None());

        Assert.Throws<ArgumentException>(invalidGeoCreator);
    }
}
