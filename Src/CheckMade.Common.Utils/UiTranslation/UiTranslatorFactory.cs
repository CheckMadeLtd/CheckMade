namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslatorFactory
{
    IUiTranslator Create(LanguageCode targetLanguage);
}

public class UiTranslatorFactory : IUiTranslatorFactory
{
    public IUiTranslator Create(LanguageCode targetLanguage)
    {
        return new UiTranslator(targetLanguage);
    }
}