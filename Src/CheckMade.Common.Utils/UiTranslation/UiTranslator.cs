using System.Text;
using System.Text.RegularExpressions;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslator
{
    string Translate(UiString uiString);
}

public sealed partial class UiTranslator(
    Option<IReadOnlyDictionary<string, string>> translationByKey,
    ILogger<UiTranslator> logger) 
    : IUiTranslator
{
    public string Translate(UiString uiString)
    {
        var translatedAll = new StringBuilder();

        foreach (var part in uiString.Concatenations)
        {
            if (part != null)
                translatedAll.Append(Translate(part));            
        }

        var unformattedTranslation = translationByKey.Match(
            dictionary => dictionary.TryGetValue(uiString.RawEnglishText, out var translation)
                ? translation
                // e.g. new U.I. text hasn't been translated; a resource file w. outdated key; use of UiNoTranslate();
                : uiString.RawEnglishText,
                   // e.g. targetLanguage is 'en'; target dictionary couldn't be created;
            () => uiString.RawEnglishText);
        
        var formattedTranslation = Attempt<string>.Run(() =>
            translatedAll + 
            string.Format(unformattedTranslation, uiString.MessageParams));
        
        return formattedTranslation.Match(
            GetFormattedTranslationWithAnySurplusParamsAppended,
            ex =>
            {
                logger.LogWarning(ex, "Failed to format translated UiString for: '{unformatted}' " +
                                      "with {paramsCount} provided string formatting parameters.", 
                    unformattedTranslation, uiString.MessageParams.Length);
                
                return GetUnformattedTranslationWithInsufficientParamsAppended();
            });

        string GetFormattedTranslationWithAnySurplusParamsAppended(string formatted)
        {
            var numberOfParamPlaceholders = MyParamPlaceholderMatcher().Matches(unformattedTranslation).Count;
            var actualNumberOfParams = uiString.MessageParams.Length; 
            var paramSurplus = actualNumberOfParams - numberOfParamPlaceholders;

            return paramSurplus > 0
                ? $"{formatted}[{string.Join("; ", uiString.MessageParams.TakeLast(paramSurplus))}]"
                : formatted;
        }

        string GetUnformattedTranslationWithInsufficientParamsAppended() =>
            $"{unformattedTranslation}[{string.Join("; ", uiString.MessageParams)}]";
    }

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex MyParamPlaceholderMatcher();
}