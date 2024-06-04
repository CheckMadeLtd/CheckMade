using CheckMade.Common.Interfaces.ExternalServices.AzureServices;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface IToModelConverterFactory
{
    IToModelConverter Create(ITelegramFilePathResolver filePathResolver);
}

public class ToModelConverterFactory(IBlobLoader blobLoader) : IToModelConverterFactory
{
    public IToModelConverter Create(ITelegramFilePathResolver filePathResolver) =>
        new ToModelConverter(filePathResolver, blobLoader);
}