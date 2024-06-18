namespace CheckMade.Common.Model.Core.Structs;

public readonly struct Latitude
{
    private const double MaxLatitude = 90;
    private const double MinLatitude = -90;
    
    private readonly double _value;

    public Latitude(double coordinate)
    {
        if (coordinate is > MaxLatitude or < MinLatitude)
        {
            throw new ArgumentException($"{coordinate} is an invalid value for latitude, which needs to be between" +
                                        $"{MinLatitude} and {MaxLatitude}.");
        }

        _value = coordinate;
    }

    public Latitude Value => _value;

    public static implicit operator Latitude(double coordinate) => new(coordinate);
    public static implicit operator double(Latitude latitude) => latitude._value;
}