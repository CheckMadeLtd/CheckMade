using CheckMade.Telegram.Function.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Startup;

public abstract class TestStartupBase
{
    protected IConfigurationRoot Config { get; private init; }
    protected string HostingEnvironment { get; private init; }
    protected ServiceCollection Services { get; } = [];
    
    protected TestStartupBase()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            // If this file can't be found we assume the test runs on GitHub Actions Runner with corresp. env. variables! 
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(); // Also includes Env Vars set in GH Actions Workflow
        Config = builder.Build();
        
        // From local.settings.json or from env variable set in GitHub Actions workflow!
        HostingEnvironment = Config.GetValue<string>("HOSTING_ENVIRONMENT")
            ?? throw new ArgumentNullException(nameof(Config), "Can't find HOSTING_ENVIRONMENT");

        ConfigureServices();
    }

    protected void ConfigureServices()
    {
        Services.ConfigureBotServices();
        Services.ConfigureBusinessServices();
    }
}