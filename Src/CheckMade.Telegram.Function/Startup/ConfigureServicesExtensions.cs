using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Persistence;
using CheckMade.Common.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Function.Startup;

// In the Unix Env. (including locally and on GitHub Runner) the var names/keys need to use '_'
// but in Azure Keyvault they need to use '-'

public static class ConfigureServicesExtensions
{
    internal static void ConfigureBotServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        var botTypes = Enum.GetNames(typeof(BotType));
        foreach (var botType in botTypes)
        {
            services.AddHttpClient($"CheckMade{botType}Bot");            
        }    
        
        services.AddSingleton<IBotClientFactory, BotClientFactory>();
        services.AddScoped<UpdateService>();
    }

    public static void ConfigurePersistenceServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        services.Add_TelegramPersistence_Dependencies();
        
        var dbConnectionString = hostingEnvironment switch
        {
            "Development" or "CI" => 
                config.GetValue<string>("PG_DB_CONNSTRING") 
                ?? throw new ArgumentNullException(nameof(hostingEnvironment), 
                    "Can't find PG_DB_CONNSTRING"),
            
            "Production" or "Staging" => 
                (Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_PRD-DB") 
                 ?? throw new ArgumentNullException(nameof(hostingEnvironment), 
                     "Can't find POSTGRESQLCONNSTR_PRD-DB"))
                .Replace("MYSECRET", config.GetValue<string>("ConnectionStrings:PRD-DB-PSW") 
                                     ?? throw new ArgumentNullException(nameof(hostingEnvironment), 
                                         "Can't find ConnectionStrings:PRD-DB-PSW")),
            
            _ => throw new ArgumentException((nameof(hostingEnvironment)))
        };
        
        services.Add_CommonPersistence_Dependencies(dbConnectionString);
    }
    
    public static void ConfigureBusinessServices(this IServiceCollection services)
    {
        services.Add_MessagingLogic_Dependencies();
    }
}