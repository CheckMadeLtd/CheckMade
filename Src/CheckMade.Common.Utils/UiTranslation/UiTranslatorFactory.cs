using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using CheckMade.Common.Utils.FpExtensions;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslatorFactory
{
    IUiTranslator Create(LanguageCode targetLanguage);
}

public sealed class UiTranslatorFactory(
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
            LanguageCode.en => Option<IReadOnlyDictionary<string, string>>.None(),
            
            LanguageCode.de => CreateTranslationDictionary().Match(
                Option<IReadOnlyDictionary<string, string>>.Some,
                // Assuming this can only be an Exception (not a BusinessError)
                ex =>
                {
                    logger.LogWarning(((ExceptionWrapper)ex).Exception,
                        $"Failed to create translation dictionary for '{_targetLanguage}'," +
                        $"and so U.I. will be English.");
                    
                    return Option<IReadOnlyDictionary<string, string>>.None();
                }),
            
            _ => throw new ArgumentOutOfRangeException(nameof(targetLanguage))
        };
        
        return new UiTranslator(translationByKey, loggerForUiTranslator);
    }

    private Result<IReadOnlyDictionary<string, string>> CreateTranslationDictionary()
    {
        return Result<IReadOnlyDictionary<string, string>>.Run(() =>
        {
            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t"
            }; 
        
            using (var reader = new StreamReader(GetTranslationResourceStream()))
            using (var csv = new CsvReader(reader, config))
            {
                while(csv.Read())
                {
                    /* Why Replace()?
                     In the .tsv file any '\n' is a string literal, while in the resulting translation dictionary
                     we need them to become actual line-breaking control characters to ensure a match against any
                     UiString.RawEnglishText in the Translate() method.
                     
                     Why support for \n needed at all?
                     Despite having introduced UiNewLine(x) to add line breaks, which does away with the need for \n in
                     the translation file, there are cases where I prefer to use """raw string literals""" to define 
                     UiStrings, where actual line breaks translate to \n in translation file. */ 
                    var enKey = csv.GetField(1)!.Replace("\\n", "\n");
                    var translation = csv.GetField(2)!.Replace("\\n", "\n");
                    
                    builder.Add(enKey, translation);
                }
            }

            return builder.ToImmutable();
        });
    }

    private Stream GetTranslationResourceStream()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        var targetLanguagesResourceName =
            $"{typeof(UiTranslatorFactory).Namespace}.TargetLanguages.{_targetLanguage}.tsv";

        return assembly.GetManifestResourceStream(targetLanguagesResourceName)!;
    }
}