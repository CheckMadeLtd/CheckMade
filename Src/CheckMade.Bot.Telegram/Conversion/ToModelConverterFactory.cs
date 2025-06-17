using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.ExternalServices.AzureServices;
using CheckMade.Abstract.Domain.Interfaces.ExternalServices.Utils;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using Microsoft.Extensions.Logging;

namespace CheckMade.Bot.Telegram.Conversion;

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