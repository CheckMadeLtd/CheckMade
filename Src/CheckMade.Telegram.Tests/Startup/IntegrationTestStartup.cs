using CheckMade.Telegram.Function.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Startup;

// ReSharper disable once ClassNeverInstantiated.Global
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