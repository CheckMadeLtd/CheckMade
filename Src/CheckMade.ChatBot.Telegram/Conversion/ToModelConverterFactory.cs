using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Domain.Interfaces.ExternalServices.Utils;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Telegram.Conversion;

public interface IToModelConverterFactory
{
    IToModelConverter Create(ITelegramFilePathResolver filePathResolver);
}

public sealed class ToModelConverterFactory(
    IBlobLoader blobLoader,
    IHttpDownloader downloader,
    IAgentRoleBindingsRepository roleBindingsRepo,
    IDomainGlossary domainGlossary,
    ILogger<ToModelConverter> logger) 
    : IToModelConverterFactory
{
    public IToModelConverter Create(ITelegramFilePathResolver filePathResolver) =>
        new ToModelConverter(filePathResolver, blobLoader, downloader, roleBindingsRepo, domainGlossary, logger);
}