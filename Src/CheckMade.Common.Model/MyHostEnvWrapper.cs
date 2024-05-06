namespace CheckMade.Common.Model;

public class MyHostEnvWrapper
{
    private string? _value;

    public MyHostEnvWrapper(string? value)
    {
        SetValue(value);
    }

    private void SetValue(string? value)
    {
        if (value is "Development" or "Staging" or "CI" or "Production")
        {
            _value = value;
        }
        else
        {
            throw new ArgumentException("Not a valid hosting environment");
        }
    }
    
    public static implicit operator string?(MyHostEnvWrapper restrictedString)
    {
        return restrictedString._value;
    }
    
    public static implicit operator MyHostEnvWrapper(string? value)
    {
        return new MyHostEnvWrapper(value);
    }

    public override string? ToString()
    {
        return _value;
    }
}