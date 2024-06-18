namespace CheckMade.Common.Model.Core.Structs;

public readonly struct EmailAddress
{
    private readonly string _value;

    public EmailAddress(string value)
    {
        if (!InputValidator.IsValidEmailAddress(value))
        {
            throw new ArgumentException($"Invalid email address: {value}");
        }

        _value = value;
    }

    public override string ToString() => _value;
}