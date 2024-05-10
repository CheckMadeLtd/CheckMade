using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Function.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Telegram.Tests.Startup;

public abstract class TestStartupBase
{
    protected IConfigurationRoot Config { get; private init; }
    protected string HostingEnvironment { get; private init; }
    internal ServiceCollection Services { get; } = [];
    
    protected TestStartupBase()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            // If this file can't be found we assume the test runs on GitHub Actions Runner with corresp. env. variables! 
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            // This config (the secrets.json of the main Telegram project) gets ignored on the GitHub Actions Runner
            .AddUserSecrets("dd4f1069-ae94-4987-9751-690e8da6f3c0") 
            // This also includes Env Vars set in GitHub Actions Workflow
            .AddEnvironmentVariables();
        Config = builder.Build();
        
        // This is taken either from local.settings.json or from env variable set in GitHub Actions workflow!
        HostingEnvironment = Config.GetValue<string>("HOSTING_ENVIRONMENT")
            ?? throw new ArgumentNullException(nameof(Config), "Can't find HOSTING_ENVIRONMENT");
    }

    protected void ConfigureServices()
    {
        RegisterBaseServices();
        RegisterTestTypeSpecificServices();
    }

    private void RegisterBaseServices()
    {
        Services.AddLogging(config =>
        {
            config.ClearProviders();
            config.AddConsole(); 
            config.AddDebug(); 
        });
        
        Services.AddScoped<IBotUpdateHandler, BotUpdateHandler>();
        Services.AddSingleton<ITestUtils, TestUtils>();
        
        Services.ConfigureUtilityServices();
        Services.ConfigureBusinessServices();
        Services.ConfigureNetworkRetryPolicyAndServices();
    }

    protected abstract void RegisterTestTypeSpecificServices();
}