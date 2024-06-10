namespace CheckMade.Common.Model.Core;

// ToDo: instead of double, use custom type that can only take valid values, with implicit operators
public record Geo(double Latitude, double Longitude, Option<float> UncertaintyRadiusInMeters);