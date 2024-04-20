using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using CheckMade.Chat.Logic;
using CheckMade.Chat.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Telegram.Bot;

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
        else if (environment.IsProduction())
        {
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(new Uri("https://keyvault-zv.vault.azure.net/"), credential);
            config.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
        }

        /* According to GPT4, the EnvironmentVariables should include the settings from 'Configuration' in the Azure App
         and they would take precedence over any local .json setting files (which are added by default via the above */
        config.AddEnvironmentVariables();
    })
    .ConfigureLogging((hostContext, logging) =>
    {
        var loggerConfig = new LoggerConfiguration();

        var humanReadability = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] (PID:{ProcessId})" +
                             " {Message:lj}{NewLine}{Exception}";
        
        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            loggerConfig
                .MinimumLevel.Override("CheckMade.Chat.Telegram", LogEventLevel.Debug)
                
                .Enrich.WithProcessId()
                // .Enrich.WithProperty("PlaceholderProp", "PlaceholderValue")
                .Enrich.FromLogContext()
                
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    "../../../logs/dev-for-machine.log",
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    rollingInterval: RollingInterval.Day)
                
                .WriteTo.File(
                    "../../../logs/dev-for-human.log", 
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: humanReadability);
        }

        Log.Logger = loggerConfig.CreateLogger();
        logging.AddSerilog(Log.Logger, true);
        
        /* Notes about the above setup
         a) the Function writes to Application Insights even without specifying that sink,
            thanks to host.json and Azure Function default settings. However, specifying the Application Insights
            sink for Serilog would give me more fine-grained control (e.g. of LogLevels) and consistent logging
            across sinks and configuration all in one palce (here) etc. if needed in the future
        b) not writing to Console via SeriLog but relying on Azure Function's default logging, which includes
            colour coding etc. and default LogLevels for system components and 'Information' for my code. This avoids
            duplicates from SeriLog and Azure, and which are hard to suppress. 
            --> For seeing logs that follow my configuration, need to use files. */   
    })
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;

        var tgToken = config.GetValue<string>("TelegramBotConfiguration:SubmissionsBotToken");
    
        services.AddHttpClient("telegram_submissions_client")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(tgToken, httpClient));

        services.AddScoped<UpdateService>();
        services.Add_MessagingLogic_Dependencies();
    })
    .Build();

host.Run();
