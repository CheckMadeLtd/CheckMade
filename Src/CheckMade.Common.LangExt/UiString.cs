using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;

namespace CheckMade.Common.LangExt;

public sealed record UiString(
    IReadOnlyCollection<UiString?> Concatenations,
    string RawEnglishText,
    object[] MessageParams)
{
    [StringFormatMethod("uiString")]
    public static UiString Ui(string uiString, params object[] messageParams) 
        => new(new List<UiString>(), uiString, messageParams);

    public static UiString Ui() => Ui(string.Empty);

    /* When we need to wrap a variable (e.g. ex.Message) as a UiString. It would have been spelled out as a 
    Ui("string literal") elsewhere in the code. By using a dedicated method, our code parser can more easily skip
    these instances when compiling the list of all 'keys' to be used in translation resource files. */ 
    public static UiString UiIndirect(string dynamicString) => Ui(dynamicString, []); 
    
    // Only use this for names/labels etc. but not for strings (like "n/a") which are similar between languages. 
    public static UiString UiNoTranslate(string uiString) => Ui("{0}", uiString);

    public static UiString UiNewLines(int newLineCount) => 
        UiConcatenate(Enumerable.Repeat(UiNoTranslate("\n"), newLineCount).ToArray());
    
    public static UiString UiConcatenate(params UiString?[] uiStrings) => 
        UiConcatenate(uiStrings.ToImmutableArray());

    public static UiString UiConcatenate(IReadOnlyCollection<UiString?> uiStrings) => 
        new(uiStrings.ToImmutableArray(), string.Empty, []);

    // For when I need to convert a UiString with Message Params back to a fully formatted string (see usage examples)
    public string GetFormattedEnglish()
    {
        var allFormatted = new StringBuilder();

        allFormatted.Append(string.Format(RawEnglishText, MessageParams));

        foreach (var uiString in Concatenations)
        {
            allFormatted.Append(uiString?.GetFormattedEnglish());
        }

        return allFormatted.ToString();
    }
}
