using CheckMade.Common.Persistence;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Function.Startup;

// In the Unix Env. (including locally and on GitHub Runner) the var names/keys need to use '_'
// but in Azure Keyvault they need to use '-'

internal static class ConfigureServicesExtensions
{
    internal static void ConfigureBotClientServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        services.AddSingleton<BotTokens>(_ => PopulateBotTokens(config, hostingEnvironment));

        var botTypes = Enum.GetNames(typeof(BotType));
        foreach (var botType in botTypes)
        {
            services.AddHttpClient($"CheckMade{botType}Bot");            
        }    
        
        services.AddSingleton<IBotClientFactory, BotClientFactory>();
    }
    
    internal static void ConfigurePersistenceServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        services.Add_TelegramPersistence_Dependencies();
        
        var dbConnectionString = (string?)hostingEnvironment switch
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

    internal static void ConfigureBusinessServices(this IServiceCollection services)
    {
        services.AddSingleton<IToModelConverter, ToModelConverter>();
        services.Add_TelegramLogic_Dependencies();
    }
    
    private static BotTokens PopulateBotTokens(IConfiguration config, string hostingEnvironment) => 
        (string?)hostingEnvironment switch
        {
            "Development" => new BotTokens(
                GetBotToken(config, "DEV", BotType.Submissions),
                GetBotToken(config, "DEV", BotType.Communications),
                GetBotToken(config, "DEV", BotType.Notifications)),

            "Staging" => new BotTokens(
                GetBotToken(config, "STG", BotType.Submissions),
                GetBotToken(config, "STG", BotType.Communications),
                GetBotToken(config, "STG", BotType.Notifications)),

            "Production" => new BotTokens(
                GetBotToken(config, "PRD", BotType.Submissions),
                GetBotToken(config, "PRD", BotType.Communications),
                GetBotToken(config, "PRD", BotType.Notifications)),

            _ => throw new ArgumentException((nameof(hostingEnvironment)))
        };

    private static string GetBotToken(IConfiguration config, string envAcronym, BotType botType) =>
        config.GetValue<string>($"TelegramBotConfiguration:{envAcronym}-CHECKMADE-{botType}-BOT-TOKEN")
        ?? throw new ArgumentNullException(nameof(config), 
            $"Not found: TelegramBotConfiguration:{envAcronym}-CHECKMADE-{botType}-BOT-TOKEN");
}