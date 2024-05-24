using System.Text;
using CheckMade.Common.Interfaces;
using CheckMade.Common.LangExt;

namespace CheckMade.Common.Utils;

public class UiTranslator : IUiTranslator
{
    public string Translate(UiString uiString)
    {
        var translatedAll = new StringBuilder();
        
        foreach (var part in uiString.Concatenations)
            translatedAll.Append(Translate(part));

        var translated = uiString.RawOriginalText;

        return translatedAll + string.Format(translated, uiString.MessageParams);
    }
}