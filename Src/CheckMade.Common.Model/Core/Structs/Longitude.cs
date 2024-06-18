namespace CheckMade.Common.Model.Core.Structs;

public readonly struct Longitude
{
    private const double MaxLongitude = 180;
    private const double MinLongitude = -180;
    
    private readonly double _value;

    public Longitude(double coordinate)
    {
        if (coordinate is > MaxLongitude or < MinLongitude)
        {
            throw new ArgumentException($"{coordinate} is an invalid value for longitutde, which needs to be between" +
                                        $"{MinLongitude} and {MaxLongitude}.");
        }

        _value = coordinate;
    }

    public Longitude Value => _value;

    public static implicit operator Longitude(double coordinate) => new(coordinate);
    public static implicit operator double(Longitude longitude) => longitude._value;
}