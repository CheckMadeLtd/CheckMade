using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using CheckMade.Telegram.Function.Startup;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            var secretClient = new SecretClient(new Uri("https://keyvault-sstw4.vault.azure.net/"), credential);
            config.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
        }

        /* According to GPT4, the EnvironmentVariables should include the settings from 'Configuration' in the
         Azure App and they would take precedence over any local .json setting files (which are added by default) */
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;
        var hostingEnvironment = hostContext.HostingEnvironment.EnvironmentName;
        
        // These two are separated into two because only one of them is relevant for Integration tests
        services.ConfigureBotClientServices(config, hostingEnvironment);
        services.ConfigureBotUpdateHandlingServices();
        
        services.ConfigurePersistenceServices(config, hostingEnvironment);
        services.ConfigureUtilityServices();
        services.ConfigureBotBusinessServices();
    })
    .ConfigureLogging((hostContext, logging) =>
    {
        var config = hostContext.Configuration;
        
        var loggerConfig = new LoggerConfiguration();

        loggerConfig
            .MinimumLevel.Override("CheckMade", LogEventLevel.Debug)

            .Enrich.WithProcessId()
            .Enrich.FromLogContext();
         // .Enrich.WithProperty("PlaceholderProp", "PlaceholderValue")

        var humanReadability = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] (PID:{ProcessId}) " +
                               "{Message:lj} || SourceContext: {SourceContext} {NewLine}";
        
        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            /* Not writing to Console via SeriLog but relying on Azure Function's default logging with default
             LogLevels for system components and 'Information' for my code. This avoids duplicates from SeriLog
             and Azure which seem to be hard to suppress.
           --> Hence for seeing logs in Dev env. following my precise config, we use files rather than console. */

            loggerConfig
                    
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
        else
        {
            /* The Function writes default LogLevels to Application Insights even without specifying that sink,
            thanks to host.json and Azure Function default settings. With this default config, for logs from my own 
            code, the min LogLevel is 'Information'. System components have their own default min level.
            'SourceContext' is one of the useful items that seems NOT to be logged by default though.
            
            ==> For more fain-grained control of what goes into Application Insights, we therefore use
            SeriLog's corresponding sink here */

            var telemetryConfig = new TelemetryConfiguration
            {
                ConnectionString = config["APPLICATIONINSIGHTS_CONNECTION_STRING"]
            };
            
            loggerConfig
                .WriteTo.Console(
                    outputTemplate: humanReadability,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                
                .WriteTo.ApplicationInsights(
                    telemetryConfig, new CustomTelemetryConverter(),
                    restrictedToMinimumLevel: LogEventLevel.Information);
        }

        Log.Logger = loggerConfig.CreateLogger();
        logging.ClearProviders();
        logging.AddSerilog(Log.Logger, true);
    })
    .Build();

await host.StartAsync();

// The combination of host.Start() and host.WaitForShutdown() let's me run code HERE
// after the host started, contrary to just using Run().

await host.WaitForShutdownAsync();
