namespace CheckMade.Abstract.Domain.Model.Core.GIS;

public readonly record struct Longitude
{
    private const double MaxLongitude = 180;
    private const double MinLongitude = -180;
    
    public Longitude(double coordinate)
    {
        if (coordinate is > MaxLongitude or < MinLongitude)
        {
            throw new ArgumentException($"{coordinate} is an invalid value for longitutde, which needs to be between" +
                                        $"{MinLongitude} and {MaxLongitude}.");
        }

        Value = coordinate;
    }
    
    public double Value { get; } // don't add init; to avoid circumventing validation!
    
    public static implicit operator Longitude(double coordinate) => new(coordinate);
    public static implicit operator double(Longitude longitude) => longitude.Value;
}