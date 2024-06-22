namespace CheckMade.Common.LangExt;

public record DomainTerm
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
    
    public Enum? EnumValue { get; }
    public Type? EnumType { get; }
    public Type? TypeValue { get; }

    public static DomainTerm Dt(Enum enumValue) => new(enumValue);
    public static DomainTerm Dt(Type typeValue) => new(typeValue);
    
    public override string ToString() => 
        EnumValue != null 
            ? EnumValue.ToString() 
            : TypeValue!.ToString();
}