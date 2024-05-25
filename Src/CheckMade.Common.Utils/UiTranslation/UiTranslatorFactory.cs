using System.Globalization;
using System.Reflection;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslatorFactory
{
    IUiTranslator Create(LanguageCode targetLanguage);
}

public class UiTranslatorFactory(
        ILogger<UiTranslatorFactory> logger,
        ILogger<UiTranslator> loggerForUiTranslator) 
    : IUiTranslatorFactory
{
    private LanguageCode _targetLanguage;
    
    public IUiTranslator Create(LanguageCode targetLanguage)
    {
        _targetLanguage = targetLanguage;
        
        var translationByKey = _targetLanguage switch
        {

            LanguageCode.En => Option<IDictionary<string, string>>.None(),
            
            LanguageCode.De => SafelyCreateTranslationDictionary().Match(
                Option<IDictionary<string, string>>.Some,
                ex =>
                {
                    logger.LogWarning(ex.Exception, 
                        $"Failed to create translation dictionary for '{_targetLanguage}'," +
                                          $"and so U.I. will be English. Exception message: " +
                                          $"'{ex.Exception?.Message ?? ex.Error?.GetFormattedEnglish()}'");
                    
                    return Option<IDictionary<string, string>>.None();
                }),
            
            _ => throw new ArgumentOutOfRangeException(nameof(targetLanguage))
        };
        
        return new UiTranslator(translationByKey, loggerForUiTranslator);
    }

    private Attempt<IDictionary<string, string>> SafelyCreateTranslationDictionary()
    {
        return Attempt<IDictionary<string, string>>.Run(() =>
        {
            var translationByKey = new Dictionary<string, string>();
            
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t"
            }; 
        
            using (var reader = new StreamReader(GetTranslationResourceStreamOrThrow()))
            using (var csv = new CsvReader(reader, config))
            {
                while(csv.Read())
                {
                    /* Why Replace()?
                     In the .tsv file any '\n' is a string literal, while in the resulting translation dictionary
                     we need them to become actual line-breaking control characters to ensure a match against any
                     UiString.RawEnglishText in the Translate() method. */ 
                    var enKey = csv.GetField(1).Replace("\\n", "\n");
                    var translation = csv.GetField(2).Replace("\\n", "\n");
                    translationByKey[enKey] = translation;
                }
            }

            return translationByKey;
        });
    }

    private Stream GetTranslationResourceStreamOrThrow()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        var targetLanguagesResourceName =
            $"{typeof(UiTranslatorFactory).Namespace}.TargetLanguages.{_targetLanguage.ToString().ToLower()}.tsv";

        return assembly.GetManifestResourceStream(targetLanguagesResourceName)!;
    }
}