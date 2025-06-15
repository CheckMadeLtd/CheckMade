using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.Core.GIS;

public sealed record Geo(
    Latitude Latitude,
    Longitude Longitude,
    Option<double> UncertaintyRadiusInMeters);