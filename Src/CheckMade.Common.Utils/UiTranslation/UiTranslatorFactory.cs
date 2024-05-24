using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslatorFactory
{
    IUiTranslator Create(LanguageCode targetLanguage);
}

public class UiTranslatorFactory(ILogger<UiTranslator> logger) : IUiTranslatorFactory
{
    public IUiTranslator Create(LanguageCode targetLanguage)
    {
        var translationByKey = targetLanguage switch
        {

            LanguageCode.En => Option<IDictionary<string, string>>.None(),
            LanguageCode.De => Option<IDictionary<string, string>>.Some(
                CreateTranslationDictionary(targetLanguage)),
            _ => throw new ArgumentOutOfRangeException(nameof(targetLanguage))
        };
        
        return new UiTranslator(translationByKey, logger);
    }

    private IDictionary<string, string> CreateTranslationDictionary(LanguageCode targetLanguage)
    {
        throw new NotImplementedException();
    }
}