using General.Utils.Validators;

namespace CheckMade.Core.Model.Common.Actors;

public readonly record struct MobileNumber
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