using CheckMade.Common.Model.Core.Structs;

namespace CheckMade.Common.Model.Core;

public record Geo(Latitude Latitude, Longitude Longitude, Option<float> UncertaintyRadiusInMeters);