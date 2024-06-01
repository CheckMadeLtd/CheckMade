namespace CheckMade.Common.Model;

public record Geo(double Latitude, double Longitude, Option<float> UncertaintyRadiusInMeters);