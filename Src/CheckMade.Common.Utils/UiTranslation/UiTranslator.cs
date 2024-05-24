using System.Text;
using CheckMade.Common.LangExt;

namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslator
{
    string Translate(UiString uiString);
}

public class UiTranslator(LanguageCode targetLanguage) : IUiTranslator
{
    public string Translate(UiString uiString)
    {
        var translatedAll = new StringBuilder();
        
        foreach (var part in uiString.Concatenations)
            translatedAll.Append(Translate(part));

        // Will be replaced with a method of actually looking up translations in a resource file, using 
        // RawOriginalText as the look-up key
        var translated = uiString.RawOriginalText;

        return translatedAll + string.Format(translated, uiString.MessageParams);
    }
}