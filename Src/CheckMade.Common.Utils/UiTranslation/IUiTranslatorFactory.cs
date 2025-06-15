namespace CheckMade.Common.Utils.UiTranslation;

public interface IUiTranslatorFactory
{
    IUiTranslator Create(LanguageCode targetLanguage);
}
