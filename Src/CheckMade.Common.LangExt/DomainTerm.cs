using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.LangExt;

public sealed record DomainTerm
{
    public DomainTerm(Enum enumValue)
    {
        EnumValue = enumValue;
        EnumType = EnumValue.GetType();
        
        TypeValue = null;
    }

    public DomainTerm(Type typeValue)
    {
        EnumValue = null;
        EnumType = null;
        
        TypeValue = typeValue;
    }
    
    public Enum? EnumValue { get; init; }
    public Type? EnumType { get; init; }
    public Type? TypeValue { get; init; }

    public Option<bool> Toggle { get; init; } = Option<bool>.None();

    public static DomainTerm Dt(Enum enumValue) => new(enumValue);
    public static DomainTerm Dt(Type typeValue) => new(typeValue);
    
    public override string ToString() => 
        EnumValue != null 
            ? EnumValue.ToString() 
            : TypeValue!.ToString();

    public bool Equals(DomainTerm? other)
    {
        return other is not null &&
               Equals(EnumValue, other.EnumValue) &&
               EnumType == other.EnumType &&
               TypeValue == other.TypeValue;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(EnumValue, EnumType, TypeValue);
    }
}