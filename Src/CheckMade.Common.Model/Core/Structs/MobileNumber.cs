namespace CheckMade.Common.Model.Core.Structs;

public readonly struct MobileNumber
{
    private readonly string _value;

    public MobileNumber(string value)
    {
        if (!InputValidator.IsValidMobileNumber(value))
        {
            throw new ArgumentException($"Invalid mobile number: {value}");
        }

        _value = value;
    }

    public override string ToString() => _value;
}