using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.Tests.Startup.DefaultStubs;
using CheckMade.Tests.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using File = Telegram.Bot.Types.TGFile;

namespace CheckMade.Tests.Startup;

[UsedImplicitly]
public class UnitTestStartup : TestStartupBase
{
    public UnitTestStartup()
    {
        RegisterServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        RegisterTestBotClient();
        RegisterTestExternalServices();
        
        // adds stubRepos with default values
        Services.ConfigureTestRepositories();
    }

    private void RegisterTestBotClient()
    {
        Services.AddScoped<IBotClientFactory, StubBotClientFactory>(sp => 
            new StubBotClientFactory(sp.GetRequiredService<Mock<IBotClientWrapper>>()));

        /* Adding Mock<IBotClientWrapper> into the D.I. container is necessary so that I can inject the same instance
         in my tests that is also used by the StubBotClientFactory below. This way I can verify behavior on the
         mockBotClientWrapper without explicitly setting up the mock in the unit test itself.

         We choose 'AddScoped' because we want our dependencies scoped to the execution of each test method.
         That's why each test method creates its own ServiceProvider. 
         
         That prevents:

         a) interference between test runs e.g. because of shared state in some dependency (which could e.g.
         falsify Moq's behavior 'verifications'

         b) having two instanced of e.g. mockBotClientWrapper within a single test-run, when only one is expected
        */ 
        Services.AddScoped<Mock<IBotClientWrapper>>(_ =>
        {
            var mockBotClientWrapper = new Mock<IBotClientWrapper>();
            
            mockBotClientWrapper
                .Setup(x => x.GetFileAsync(It.IsNotNull<string>()))
                .ReturnsAsync(new File { FilePath = "fakeFilePath" });
            mockBotClientWrapper
                .Setup(x => x.MyBotToken)
                .Returns("fakeToken");
            
            return mockBotClientWrapper;
        });
    }

    private void RegisterTestExternalServices()
    {
        Services.AddScoped<IBlobLoader, StubBlobLoader>();
        Services.AddScoped<IHttpDownloader, StubHttpDownloader>(); 
    }
}
