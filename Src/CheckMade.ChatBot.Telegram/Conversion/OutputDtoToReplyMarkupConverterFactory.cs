using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.ChatBot.Telegram.Conversion;

public interface IOutputToReplyMarkupConverterFactory
{
    IOutputToReplyMarkupConverter Create(IUiTranslator translator, IDomainGlossary glossary);
}

public sealed class OutputToReplyMarkupConverterFactory : IOutputToReplyMarkupConverterFactory
{
    public IOutputToReplyMarkupConverter Create(IUiTranslator translator, IDomainGlossary glossary) => 
        new OutputToReplyMarkupConverter(translator, glossary);
}