using CheckMade.Common.DomainModel.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Utils;

public interface IUiTranslatorFactory
{
    IUiTranslator Create(LanguageCode targetLanguage);
}
