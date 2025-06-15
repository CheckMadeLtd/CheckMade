using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.Core.GIS;

public sealed record Geo(
    Latitude Latitude,
    Longitude Longitude,
    Option<double> UncertaintyRadiusInMeters);