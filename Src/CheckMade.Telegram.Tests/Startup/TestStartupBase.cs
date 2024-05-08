using CheckMade.Telegram.Function.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Startup;

public abstract class TestStartupBase : IDisposable, IAsyncDisposable
{
    protected IConfigurationRoot Config { get; private init; }
    protected string HostingEnvironment { get; private init; }
    protected ServiceCollection Services { get; } = [];
    
    internal ServiceProvider ServiceProvider { get; private set; } = null!;
    
    protected TestStartupBase()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            // If this file can't be found we assume the test runs on GitHub Actions Runner with corresp. env. variables! 
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            // This gets ignored on the GitHub Actions Runner
            .AddUserSecrets("dd4f1069-ae94-4987-9751-690e8da6f3c0") 
            // Also includes Env Vars set in GH Actions Workflow
            .AddEnvironmentVariables();
        Config = builder.Build();
        
        // From local.settings.json or from env variable set in GitHub Actions workflow!
        HostingEnvironment = Config.GetValue<string>("HOSTING_ENVIRONMENT")
            ?? throw new ArgumentNullException(nameof(Config), "Can't find HOSTING_ENVIRONMENT");
    }

    protected void ConfigureServices()
    {
        RegisterBaseServices();
        RegisterTestTypeSpecificServices();
        ServiceProvider = Services.BuildServiceProvider();
    }

    private void RegisterBaseServices()
    {
        Services.ConfigureBusinessServices();
    }

    protected abstract void RegisterTestTypeSpecificServices();
    
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