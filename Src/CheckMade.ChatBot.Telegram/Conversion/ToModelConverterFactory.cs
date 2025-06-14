using CheckMade.Common.DomainModel.Interfaces.ChatBotLogic;
using CheckMade.Common.DomainModel.Interfaces.ExternalServices;
using CheckMade.Common.DomainModel.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Telegram.Conversion;

public interface IToModelConverterFactory
{
    IToModelConverter Create(ITelegramFilePathResolver filePathResolver);
}

public sealed class ToModelConverterFactory(
    IBlobLoader blobLoader,
    IHttpDownloader downloader,
    ITlgAgentRoleBindingsRepository roleBindingsRepo,
    IDomainGlossary domainGlossary,
    ILogger<ToModelConverter> logger) 
    : IToModelConverterFactory
{
    public IToModelConverter Create(ITelegramFilePathResolver filePathResolver) =>
        new ToModelConverter(filePathResolver, blobLoader, downloader, roleBindingsRepo, domainGlossary, logger);
}