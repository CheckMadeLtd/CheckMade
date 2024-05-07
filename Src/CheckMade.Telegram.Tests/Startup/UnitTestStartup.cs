using CheckMade.Telegram.Interfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Telegram.Tests.Startup;

[UsedImplicitly]
public class UnitTestStartup : TestStartupBase
{
    internal ServiceProvider GetServiceProvider() => ServiceProvider;

    public UnitTestStartup()
    {
        ConfigureServices();
    }
    
    private new void ConfigureServices()
    {
        Services.AddScoped<IMessageRepo, MockMessageRepo>(_ => new MockMessageRepo(new Mock<IMessageRepo>()));
        base.ConfigureServices();
    }
}