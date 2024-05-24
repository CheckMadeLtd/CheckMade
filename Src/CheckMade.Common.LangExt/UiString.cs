using JetBrains.Annotations;

namespace CheckMade.Common.LangExt;

public record UiString(IReadOnlyCollection<UiString> Concatenations, string RawEnglishText, object[] MessageParams)
{
    [StringFormatMethod("uiString")]
    public static UiString Ui(string uiString, params object[] messageParams) 
        => new UiString(new List<UiString>(), uiString, messageParams);

    public static UiString Ui() => Ui(string.Empty);

    /* When we need to wrap a variable (e.g. ex.Message) as a UiString. It would have been spelled out as a 
    Ui("string literal") elsewhere in the code. By using a dedicated method, our code parser can more easily skip
    these instances when compiling the list of all 'keys' to be used in translation resource files. */ 
    public static UiString UiIndirect(string dynamicString) => Ui(dynamicString, []); 
    
    // Only use this for names/labels etc. but not for strings (like "n/a") which are similar between languages. 
    public static UiString UiNoTranslate(string uiString) => Ui("{0}", uiString);
    
    public static UiString UiConcatenate(params UiString[] uiStrings) => UiConcatenate(uiStrings.ToList());
    
    public static UiString UiConcatenate(IEnumerable<UiString> uiStrings) => 
        new UiString(uiStrings.ToList(), string.Empty, []);
}
