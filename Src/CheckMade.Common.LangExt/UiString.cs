using JetBrains.Annotations;

namespace CheckMade.Common.LangExt;

public record UiString(IReadOnlyCollection<UiString> Concatenations, string RawOriginalText, object[] MessageParams)
{
    [StringFormatMethod("uiString")]
    public static UiString Ui(string uiString, params object[] messageParams) 
        => new UiString(new List<UiString>(), uiString, messageParams);

    public static UiString Ui() => Ui(string.Empty);
    
    // Only use this for names/labels etc. but not for strings (like "n/a") which are similar between languages. 
    public static UiString UiUntranslated(string uiString) => Ui("{0}", uiString);
    
    public static UiString UiConcatenate(params UiString[] uiStrings) => UiConcatenate(uiStrings.ToList());
    public static UiString UiConcatenate(IEnumerable<UiString> uiStrings) => new UiString(uiStrings.ToList(), 
        string.Empty, Array.Empty<object>());
}
