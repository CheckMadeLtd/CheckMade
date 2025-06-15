namespace CheckMade.Common.DomainModel.Data.Core.GIS;

public readonly record struct Latitude
{
    private const double MaxLatitude = 90;
    private const double MinLatitude = -90;

    public Latitude(double coordinate)
    {
        if (coordinate is > MaxLatitude or < MinLatitude)
        {
            throw new ArgumentException($"{coordinate} is an invalid value for latitude, which needs to be between" +
                                        $"{MinLatitude} and {MaxLatitude}.");
        }

        Value = coordinate;
    }

    public double Value { get; } // don't add init; to avoid circumventing validation!

    public static implicit operator Latitude(double coordinate) => new(coordinate);
    public static implicit operator double(Latitude latitude) => latitude.Value;
}