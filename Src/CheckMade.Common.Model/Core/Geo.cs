using CheckMade.Common.Model.Core.Structs;

namespace CheckMade.Common.Model.Core;

public sealed record Geo(
    Latitude Latitude,
    Longitude Longitude,
    Option<double> UncertaintyRadiusInMeters);