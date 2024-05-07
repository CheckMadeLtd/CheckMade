using CheckMade.Telegram.Function.Startup;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Startup;

[UsedImplicitly]
public class IntegrationTestStartup : TestStartupBase, IDisposable, IAsyncDisposable
{
    internal ServiceProvider ServiceProvider { get; set; } = null!;

    public IntegrationTestStartup()
    {
        ConfigureServices();
    }
    
    private new void ConfigureServices()
    {
        base.ConfigureServices();
        
        Services.ConfigurePersistenceServices(Config, HostingEnvironment);

        ServiceProvider = Services.BuildServiceProvider();
    }
    
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

}