using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Common.GIS;

public sealed record Geo(
    Latitude Latitude,
    Longitude Longitude,
    Option<double> UncertaintyRadiusInMeters);