namespace CheckMade.Common.LangExt;

public record EnumWithType(Enum Value)
{
    public static EnumWithType Et(Enum value) => new(value);
    public Type Type { get; } = Value.GetType();
    public override string ToString() => Value.ToString();
}