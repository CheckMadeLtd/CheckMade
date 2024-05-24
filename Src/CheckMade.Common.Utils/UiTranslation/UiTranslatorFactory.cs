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
                    logger.LogWarning(ex, $"Failed to create translation dictionary for '{_targetLanguage}'," +
                                          $"and so U.I. will be English. Exception message: '{ex.Message}'");
                    
                    return Option<IDictionary<string, string>>.None();
                }),
            
            _ => throw new ArgumentOutOfRangeException(nameof(_targetLanguage))
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
                    var enKey = csv.GetField(1);
                    var translation = csv.GetField(2);
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