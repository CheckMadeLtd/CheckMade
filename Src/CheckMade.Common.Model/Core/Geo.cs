namespace CheckMade.Common.Model.Core;

// ToDo: a) instead of double, use custom type that can only take valid values, with implicit operators
// b) use struct instead of record for value-type semantics?!!
public record Geo(double Latitude, double Longitude, Option<float> UncertaintyRadiusInMeters);