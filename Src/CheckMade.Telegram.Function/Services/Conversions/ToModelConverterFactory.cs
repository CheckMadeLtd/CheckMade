using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Utils.Generic;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface IToModelConverterFactory
{
    IToModelConverter Create(ITelegramFilePathResolver filePathResolver);
}

public class ToModelConverterFactory(IBlobLoader blobLoader, IHttpDownloader downloader) : IToModelConverterFactory
{
    public IToModelConverter Create(ITelegramFilePathResolver filePathResolver) =>
        new ToModelConverter(filePathResolver, blobLoader, downloader);
}