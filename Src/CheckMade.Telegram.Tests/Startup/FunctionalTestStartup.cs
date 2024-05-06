using CheckMade.Telegram.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Telegram.Tests.Startup;

// ReSharper disable once ClassNeverInstantiated.Global
public class FunctionalTestStartup : TestStartupBase
{
    internal ServiceProvider GetServiceProvider() => ServiceProvider;

    public FunctionalTestStartup()
    {
        ConfigureServices();
    }
    
    private new void ConfigureServices()
    {
        Services.AddScoped<IMessageRepo, MockMessageRepo>(_ => 
            new MockMessageRepo(new Mock<IMessageRepo>()));
        base.ConfigureServices();
    }
}