using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface IOutputToReplyMarkupConverterFactory
{
    IOutputToReplyMarkupConverter Create(IUiTranslator translator);
}

public class OutputToReplyMarkupConverterFactory : IOutputToReplyMarkupConverterFactory
{
    public IOutputToReplyMarkupConverter Create(IUiTranslator translator) => 
        new OutputToReplyMarkupConverter(translator);
}