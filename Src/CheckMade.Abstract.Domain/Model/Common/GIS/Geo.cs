using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Common.GIS;

public sealed record Geo(
    Latitude Latitude,
    Longitude Longitude,
    Option<double> UncertaintyRadiusInMeters);