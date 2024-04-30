using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace CheckMade.Chat.Telegram;

[UsedImplicitly] // Azure Functions Runtime requires public entry point
public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private static IHostBuilder CreateHostBuilder(string[] args)
    {
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
                    var secretClient = new SecretClient(new Uri("https://keyvault-cyqj1.vault.azure.net/"), credential);
                    config.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
                }

                /* The EnvironmentVariables should include the settings from 'Configuration' in the Azure App/Function
                and they would take precedence over any local .json setting files */
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.ConfigureAppServices(hostContext.Configuration, hostContext.HostingEnvironment.EnvironmentName);
                services.ConfigurePersistenceServices(hostContext.Configuration, hostContext.HostingEnvironment.EnvironmentName);
                services.ConfigureBusinessServices();
                services.AddApplicationInsightsTelemetry();
            })
            .UseSerilog((hostContext, services, loggerConfig) =>
            {
                var humanReadability = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] (PID:{ProcessId}) " +
                                       "{Message:lj} {SourceContext} {NewLine}";

                loggerConfig
                    .MinimumLevel.Override("CheckMade", LogEventLevel.Debug)
                    .Enrich.WithProcessId()
                    // .Enrich.WithProperty("PlaceholderProp", "PlaceholderValue")
                    .Enrich.FromLogContext();
                
                if (hostContext.HostingEnvironment.IsDevelopment())
                {
                    /* Not writing to Console via SeriLog but relying on Azure Function's default logging with default LogLevels
                    for system components and 'Information' for my code. This avoids duplicates from SeriLog and Azure
                    which seem to be hard to suppress.
                    --> For seeing logs in Dev env. that follow my exact configuration, use files rather than console. */   
                    
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
                    /* the Function writes default LogLevels to Application Insights even without specifying that sink,
                    thanks to host.json and Azure Function default settings. For logs from my own code, the min LogLevel is
                    'Information'. System components have their own default min level. 'SourceContext' is one of the useful
                    items that seems NOT to be logged by default. Hence, for more fain-grained control of what goes into
                    Application Insights, adding SeriLog's corresponding sink. */

                    loggerConfig
                        .WriteTo.ApplicationInsights(
                            services.GetRequiredService<TelemetryConfiguration>(), 
                            new TraceTelemetryConverter(),
                            LogEventLevel.Information);
                }
            });

        return host;
    } 
}
