using General.Utils.Validators;

namespace CheckMade.Core.Model.Common.Actors;

public readonly record struct EmailAddress
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