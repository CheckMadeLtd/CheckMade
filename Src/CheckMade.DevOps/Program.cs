using CheckMade.Common.Utils;
using CheckMade.DevOps.DataMigration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

string operation;

if (args.Length == 0)
{
    Console.Error.WriteLine($"Argument '{nameof(operation)}' is required to launch this app.");
    Environment.Exit(1);
}

operation = args[0];

var config = BuildConfigurationRoot();
var services = GetServiceCollectionWithBasics();

switch (operation)
{
    case "mig":
        string migDbTargetEnvironment;
        string migIndex;
        
        if (args.Length != 3)
        {
            Console.Error.WriteLine($"2 arguments are required to launch the 'mig' operation:\n" +
                              $"1) {nameof(migDbTargetEnvironment)} ('dev' or 'prd'),\n" +
                              $"2) {nameof(migIndex)} (in format 'xxxx')");
            Environment.Exit(1);
        }

        migDbTargetEnvironment = args[1];
        
        if (migDbTargetEnvironment is not ("dev" or "prd"))
        {
            Console.Error.WriteLine($"Not a valid data migration target environment: '{migDbTargetEnvironment}'. " +
                              $"Choose 'dev' or 'prd'.");
            Environment.Exit(1);
        }

        migIndex = args[2];

        var migStarter = new DataMigrationStartup(services, config, migDbTargetEnvironment, migIndex);
        await migStarter.StartAsync();
        
        break;
    
    default:
        Console.Error.WriteLine($"No valid {nameof(operation)} was selected. Choose from: 'mig'.");
        Environment.Exit(1);
        break;
}

return;

static IConfigurationRoot BuildConfigurationRoot()
{
    var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));

    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(projectRoot)
        .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    return configBuilder.Build();
}

static IServiceCollection GetServiceCollectionWithBasics()
{
    var services = new ServiceCollection();

    services.AddLogging(loggingConfig =>
    {
        loggingConfig.ClearProviders();
        loggingConfig.AddConsole(); 
        loggingConfig.AddDebug(); 
    });
    
    services.Add_CommonUtils_Dependencies();

    return services;
}