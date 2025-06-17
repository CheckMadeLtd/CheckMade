namespace General.Utils.Validators;

public static class EnumChecker
{
    public static bool IsDefined<TEnum>(TEnum uncertainEnum) where TEnum : Enum
    {
        return IsFlaggedEnum<TEnum>() switch
        {
            false => Enum.IsDefined(typeof(TEnum), uncertainEnum),
            
            // Enum.IsDefined doesn't work for flagged Enums, hence this trick is needed
            true => !decimal.TryParse(uncertainEnum.ToString(), out _)
        };
    }
    
    private static bool IsFlaggedEnum<TEnum>() where TEnum : Enum
    {
        return Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute));
    }
}
