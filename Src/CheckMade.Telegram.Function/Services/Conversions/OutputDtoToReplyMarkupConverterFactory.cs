using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface IOutputDtoToReplyMarkupConverterFactory
{
    IOutputDtoToReplyMarkupConverter Create(IUiTranslator translator);
}

public class OutputDtoToReplyMarkupConverterFactory : IOutputDtoToReplyMarkupConverterFactory
{
    public IOutputDtoToReplyMarkupConverter Create(IUiTranslator translator) => 
        new OutputDtoToReplyMarkupConverter(translator);
}