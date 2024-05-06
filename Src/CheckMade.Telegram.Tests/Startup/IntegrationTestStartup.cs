using CheckMade.Telegram.Function.Startup;
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
        Services.ConfigurePersistenceServices(Config, Env);
        base.ConfigureServices();
    }
}