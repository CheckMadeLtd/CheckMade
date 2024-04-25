using CheckMade.Chat.Telegram;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Chat.Tests;

[UsedImplicitly]
public class TestStartup : IDisposable, IAsyncDisposable
{
    internal ServiceProvider ServiceProvider { get; }

    public TestStartup()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        
        var services = new ServiceCollection();
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            // If this file can't be found: assuming test runs on GitHub Actions Runner with corresp. env. variables! 
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        var config = builder.Build();
        
        // From local.settings.json or from env variable set in GitHub Actions workflow!
        var env = config.GetValue<string>("HOSTING_ENVIRONMENT");

        services.ConfigurePersistenceServices(config, env);        
        services.ConfigureBusinessServices();
        
        ServiceProvider = services.BuildServiceProvider();
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