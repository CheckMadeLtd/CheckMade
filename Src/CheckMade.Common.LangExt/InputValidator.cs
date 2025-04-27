using System.Text.RegularExpressions;

namespace CheckMade.Common.LangExt;

public static partial class InputValidator
{
    private static readonly Regex TokenRegex = MyTokenRegex();
    private static readonly Regex EmailRegex = MyEmailRegex();
    private static readonly Regex MobileNoRegex = MyMobileNoRegex();
    
    public static string GetTokenFormatExample() => "ABC123";

    public static bool IsValidToken(string token) => !string.IsNullOrEmpty(token) 
                                                     && TokenRegex.IsMatch(token);
    
    public static bool IsValidEmailAddress(string emailAddress) => !string.IsNullOrEmpty(emailAddress) 
                                                                   && EmailRegex.IsMatch(emailAddress);
    
    public static bool IsValidMobileNumber(string mobileNumber) => !string.IsNullOrEmpty(mobileNumber) 
                                                                   && MobileNoRegex.IsMatch(mobileNumber);

    [GeneratedRegex(@"^\+\d+$")]
    private static partial Regex MyMobileNoRegex();
    
    [GeneratedRegex(@"^(?!.*\.\.)(?!\.)[^\s@]+(?<!\.)@[^\s@]+\.[^\s@]+$")]
    private static partial Regex MyEmailRegex();
    
    [GeneratedRegex("^[a-zA-Z0-9]{6}$")]
    private static partial Regex MyTokenRegex();
}