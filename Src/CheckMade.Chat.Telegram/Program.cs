using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using CheckMade.Chat.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

IHostEnvironment environment;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        environment = hostContext.HostingEnvironment;
        Console.Out.WriteLine($"Current HostingEnvironment is '{environment.EnvironmentName}'");

        if (environment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
        else if (environment.IsProduction() || environment.IsStaging())
        {
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(new Uri("https://keyvault-4txtr.vault.azure.net/"), credential);
            config.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
        }

        /* According to GPT4, the EnvironmentVariables should include the settings from 'Configuration' in the Azure App
         and they would take precedence over any local .json setting files (which are added by default via the above */
        config.AddEnvironmentVariables();
    })
    .ConfigureLogging((hostContext, logging) =>
    {
        var loggerConfig = new LoggerConfiguration();

        var humanReadability = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] (PID:{ProcessId}) " +
                               "{Message:lj} {SourceContext} {NewLine}";
        
        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            loggerConfig
                .MinimumLevel.Override("CheckMade", LogEventLevel.Debug)
                
                .Enrich.WithProcessId()
                // .Enrich.WithProperty("PlaceholderProp", "PlaceholderValue")
                .Enrich.FromLogContext()
                
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    "../../../logs/machine/devlogs-.log",
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    rollingInterval: RollingInterval.Day)
                
                .WriteTo.File(
                    "../../../logs/human/devlogs-.log", 
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: humanReadability);
        }

        Log.Logger = loggerConfig.CreateLogger();
        logging.AddSerilog(Log.Logger, true);
        
        /* Notes about the above setup

        a) the Function writes default LogLevels to Application Insights even without specifying that sink,
            thanks to host.json and Azure Function default settings. For logs from my own code, the min LogLevel is
            'Information'. System components have their own default min level. 'SourceContext' is one of the useful 
            items that seems NOT to be logged by default. For more fain-grained control of what goes into
            Application Insights, use SeriLog's corresponding sink and then e.g. MinimumLevel.Override.
        
        b) not writing to Console via SeriLog but relying on Azure Function's default logging with default LogLevels
            for system components and 'Information' for my code. This avoids duplicates from SeriLog and Azure
            which seem to be hard to suppress.  
            --> For seeing logs in Dev env. that follow my exact configuration, use files rather than console. 
        */   
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.ConfigureAppServices(hostContext.Configuration, hostContext.HostingEnvironment.EnvironmentName);
        services.ConfigurePersistenceServices(hostContext.Configuration, hostContext.HostingEnvironment.EnvironmentName);
        services.ConfigureBusinessServices();
    })
    .Build();

host.Run();
