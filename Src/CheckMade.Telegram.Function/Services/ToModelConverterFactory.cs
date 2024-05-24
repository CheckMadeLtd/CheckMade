using CheckMade.Common.Utils;

namespace CheckMade.Telegram.Function.Services;

public interface IToModelConverterFactory
{
    IToModelConverter Create(ITelegramFilePathResolver filePathResolver, IUiTranslator translator);
}

public class ToModelConverterFactory : IToModelConverterFactory
{
    public IToModelConverter Create(ITelegramFilePathResolver filePathResolver, IUiTranslator translator) =>
        new ToModelConverter(filePathResolver, translator);
}