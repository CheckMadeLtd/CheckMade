using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Function.Services.Conversion;

public interface IToModelConverterFactory
{
    IToModelConverter Create(ITelegramFilePathResolver filePathResolver);
}

internal class ToModelConverterFactory(
        IBlobLoader blobLoader,
        IHttpDownloader downloader,
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILogger<ToModelConverter> logger) 
    : IToModelConverterFactory
{
    public IToModelConverter Create(ITelegramFilePathResolver filePathResolver) =>
        new ToModelConverter(filePathResolver, blobLoader, downloader, roleBindingsRepo, logger);
}