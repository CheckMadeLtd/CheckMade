using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Interfaces;
using CheckMade.Tests.Startup.DefaultMocks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Tests.Startup;

[UsedImplicitly]
public class UnitTestStartup : TestStartupBase
{
    public UnitTestStartup()
    {
        ConfigureServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        Services.AddScoped<IMessageRepository, MockMessageRepository>(_ => 
            new MockMessageRepository(new Mock<IMessageRepository>()));
        
        /* Adding Mock<IBotClientWrapper> into the D.I. container is necessary so that I can inject the same instance
         in my tests that is also used by the MockBotClientFactory below. This way I can verify behaviour on the 
         mockBotClientWrapper without explicitly setting up the mock in the unit test itself.
         
         We choose 'AddScoped' because we want our dependencies scoped to the execution of each test method. 
         That's why each test method creates its own ServiceProvider. That prevents:
         
         a) interference between test runs e.g. because of shared state in some dependency (which could e.g. 
         falsify Moq's behaviour 'verifications'
         
         b) having two instanced of e.g. mockBotClientWrapper within a single test-run, when only one is expected
        */ 
        
        Services.AddScoped<Mock<IBotClientWrapper>>(_ =>
        {
            var mockBotClient = new Mock<IBotClientWrapper>();
            
            mockBotClient
                .Setup(x => x.GetFileOrThrowAsync(It.IsNotNull<string>()))
                .ReturnsAsync(new File { FilePath = "fakeFilePath" });
            
            mockBotClient
                .Setup(x => x.BotToken).Returns("fakeToken");

            return mockBotClient;
        });
        
        Services.AddScoped<IBotClientFactory, MockBotClientFactory>(sp => 
            new MockBotClientFactory(sp.GetRequiredService<Mock<IBotClientWrapper>>().Object));
    }
}
