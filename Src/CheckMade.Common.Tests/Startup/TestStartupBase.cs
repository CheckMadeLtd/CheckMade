using CheckMade.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Tests.Startup;

public abstract class TestStartupBase
{
    protected IConfigurationRoot Config { get; private init; }
    internal ServiceCollection Services { get; } = [];
    
    protected TestStartupBase()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            // If this file can't be found we assume the test runs on GitHub Actions Runner with corresp. env. variables! 
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            // This also includes Env Vars set in GitHub Actions Workflow
            .AddEnvironmentVariables();
        Config = configBuilder.Build();
    }

    protected void ConfigureServices()
    {
        RegisterBaseServices();
        RegisterTestTypeSpecificServices();
    }

    private void RegisterBaseServices()
    {
        Services.AddLogging(loggingConfig =>
        {
            loggingConfig.ClearProviders();
            loggingConfig.AddConsole(); 
            loggingConfig.AddDebug(); 
        });
        
        Services.AddSingleton<ITestUtils, TestUtils>();
        Services.Add_CommonUtils_Dependencies();
    }

    protected abstract void RegisterTestTypeSpecificServices();
}