using System.Text;
using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Integration;

public class ConverterIntegrationTests
{
    private IServiceProvider? _services;
    
    [Fact]
    public async Task ConvertToModelAsync_HasCorrectInternalAttachmentUri_ForUpdateWithAttachmentInOperationsBot()
    {
        var startup = new IntegrationTestStartup();
        if (startup.HostingEnvironment == "CI")
            return;
        
        _services = startup.Services.BuildServiceProvider();
        var updateGenerator = _services.GetRequiredService<ITelegramUpdateGenerator>();
        const string realFileIdOnDevOperationsBot = // uploaded on 04/06/2024
            "BQACAgQAAxkBAAMvZl9iHPHKeRre-ldIyMhLcEvi6a8AAi0gAALxa_lS9Z28FPz-17Q1BA";
        const string realFileUtf8Content = "Text for an integration test.";
        
        var updateWithAttachment = 
            updateGenerator.GetValidTelegramDocumentMessage(
                fileId: realFileIdOnDevOperationsBot);
        
        var blobLoader = _services.GetRequiredService<IBlobLoader>();
        var botClientFactory = _services.GetRequiredService<IBotClientFactory>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        
        var converter = converterFactory.Create(
            new TelegramFilePathResolver(
                botClientFactory.CreateBotClient(Operations)));
        
        var actualModel = 
            await converter.ConvertToModelAsync(
                updateWithAttachment, Operations);
    
        var (downloadedStream, _) = 
            await blobLoader.DownloadBlobAsync(
                actualModel.GetValueOrThrow().Details.AttachmentInternalUri.GetValueOrThrow());
        
        var downloadedContent = Encoding.UTF8.GetString(downloadedStream.ToArray());
        
        Assert.Equal(
            realFileUtf8Content,
            downloadedContent);
    }
}