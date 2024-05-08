using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Tests.Startup.DefaultMocks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Telegram.Tests.Startup;

[UsedImplicitly]
public class UnitTestStartup : TestStartupBase
{
    public UnitTestStartup()
    {
        ConfigureServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        Services.AddScoped<IMessageRepo, MockMessageRepo>(_ => new MockMessageRepo(new Mock<IMessageRepo>()));
        
        /* Adding it into the D.I. container is necessary so that I can inject the same instance in my tests that is
         also used by the MockBotClientFactory below. This way I can verify behaviour on the mockBotClientWrapper
         without explicitly setting up the mock in the unit test itself */ 
        var mockBotClientWrapper = new Mock<IBotClientWrapper>();
        Services.AddScoped(_ => mockBotClientWrapper);
        
        Services.AddScoped<IBotClientFactory, MockBotClientFactory>(_ => 
            new MockBotClientFactory(mockBotClientWrapper.Object));
    }
}
