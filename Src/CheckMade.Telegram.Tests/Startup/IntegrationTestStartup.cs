using CheckMade.Telegram.Function.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Startup;

[UsedImplicitly]
public class IntegrationTestStartup : TestStartupBase
{
    internal ServiceProvider GetServiceProvider() => ServiceProvider;

    public IntegrationTestStartup()
    {
        ConfigureServices();
    }
    
    private new void ConfigureServices()
    {
        Services.ConfigurePersistenceServices(Config, HostingEnvironment);
        base.ConfigureServices();
    }
}