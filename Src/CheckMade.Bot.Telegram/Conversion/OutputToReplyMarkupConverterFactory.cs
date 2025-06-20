using CheckMade.Core.ServiceInterfaces.Bot;
using General.Utils.UiTranslation;

namespace CheckMade.Bot.Telegram.Conversion;

public interface IOutputToReplyMarkupConverterFactory
{
    IOutputToReplyMarkupConverter Create(IUiTranslator translator, IDomainGlossary glossary);
}

public sealed class OutputToReplyMarkupConverterFactory : IOutputToReplyMarkupConverterFactory
{
    public IOutputToReplyMarkupConverter Create(IUiTranslator translator, IDomainGlossary glossary) => 
        new OutputToReplyMarkupConverter(translator, glossary);
}