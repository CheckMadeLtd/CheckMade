using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslatorFactory
{
    IUiTranslator Create(LanguageCode targetLanguage);
}

public class UiTranslatorFactory(
        string targetLanguagesDir,
        ILogger<UiTranslatorFactory> logger,
        ILogger<UiTranslator> loggerForUiTranslator) 
    : IUiTranslatorFactory
{
    public IUiTranslator Create(LanguageCode targetLanguage)
    {
        var translationByKey = targetLanguage switch
        {

            LanguageCode.En => Option<IDictionary<string, string>>.None(),
            
            LanguageCode.De => SafelyCreateTranslationDictionary(targetLanguage).Match(
                Option<IDictionary<string, string>>.Some,
                ex =>
                {
                    logger.LogWarning(ex, $"Failed to create translation dictionary for '{targetLanguage}'," +
                                          $"and so U.I. will be English. Exception message: '{ex.Message}'");
                    
                    return Option<IDictionary<string, string>>.None();
                }),
            
            _ => throw new ArgumentOutOfRangeException(nameof(targetLanguage))
        };
        
        return new UiTranslator(translationByKey, loggerForUiTranslator);
    }

    private Attempt<IDictionary<string, string>> SafelyCreateTranslationDictionary(LanguageCode targetLanguage)
    {
        return Attempt<IDictionary<string, string>>.Run(() =>
        {
            var translationByKey = new Dictionary<string, string>();

            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t"
            }; 
        
            using (var reader = new StreamReader(Path.Combine(targetLanguagesDir, $"{targetLanguage}.tsv")))
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
}