using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.ChatBot.Function.Services.Conversion;

public interface IOutputToReplyMarkupConverterFactory
{
    IOutputToReplyMarkupConverter Create(IUiTranslator translator);
}

public class OutputToReplyMarkupConverterFactory : IOutputToReplyMarkupConverterFactory
{
    public IOutputToReplyMarkupConverter Create(IUiTranslator translator) => 
        new OutputToReplyMarkupConverter(translator);
}