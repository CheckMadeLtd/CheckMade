using System.Text;
using CheckMade.Common.LangExt;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslator
{
    string Translate(UiString uiString);
}

public class UiTranslator(
        Option<IDictionary<string, string>> translationByKey,
        ILogger<UiTranslator> logger) 
    : IUiTranslator
{
    public string Translate(UiString uiString)
    {
        var translatedAll = new StringBuilder();
        
        foreach (var part in uiString.Concatenations)
            translatedAll.Append(Translate(part));
        
        var keyWithLinebreakLiterals = uiString.RawEnglishText.Replace("\n", "\\n");
        
        var translationUnformatted = translationByKey.IsSome 
            ? translationByKey.GetValueOrDefault().TryGetValue(keyWithLinebreakLiterals, out var translation)
                ? translation
                : uiString.RawEnglishText // e.g. because a new U.I. text hasn't been translated yet
            : uiString.RawEnglishText; // because targetLanguage is 'en'
        
        var translationFormatted = Attempt<string>.Run(() =>
            translatedAll + 
            string.Format(translationUnformatted, uiString.MessageParams).Replace("\\n", "\n"));
        
        return translationFormatted.Match(
            formatted => formatted,
            ex =>
            {
                logger.LogWarning(ex, "Failed to format translated UiString for: '{unformatted}' " +
                                      "with {paramsCount} provided string formatting parameters.", 
                    translationUnformatted, uiString.MessageParams.Length);
                
                return translationUnformatted;
            });
    }
}