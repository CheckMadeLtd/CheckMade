using CheckMade.Common.Persistence;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Function.Startup;

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

    internal static void ConfigureBotUpdateHandlingServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageHandler, MessageHandler>();
        services.AddScoped<IBotUpdateSwitch, BotUpdateSwitch>();
    }
    
    internal static void ConfigurePersistenceServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        services.Add_TelegramPersistence_Dependencies();
        
        var dbConnectionString = hostingEnvironment switch
        {
            "Development" or "CI" => 
                config.GetValue<string>(DbConnectionProvider.KeyToLocalDbConnStringInEnv) 
                ?? throw new InvalidOperationException(
                    $"Can't find {DbConnectionProvider.KeyToLocalDbConnStringInEnv}"),
            
            "Production" or "Staging" => 
                (Environment.GetEnvironmentVariable(DbConnectionProvider.KeyToPrdDbConnStringInKeyvault) 
                 ?? throw new InvalidOperationException(
                     $"Can't find {DbConnectionProvider.KeyToPrdDbConnStringInKeyvault}"))
                .Replace(DbConnectionProvider.DbPswPlaceholderString, 
                    config.GetValue<string>(DbConnectionProvider.KeyToPrdDbPswInKeyvaultOrSecrets) 
                    ?? throw new InvalidOperationException(
                        $"Can't find {DbConnectionProvider.KeyToPrdDbPswInKeyvaultOrSecrets}")),
            
            _ => throw new ArgumentException((nameof(hostingEnvironment)))
        };
        
        services.Add_CommonPersistence_Dependencies(dbConnectionString);
    }

    internal static void ConfigureBusinessServices(this IServiceCollection services)
    {
        services.AddSingleton<IToModelConverterFactory, ToModelConverterFactory>();
        services.Add_TelegramLogic_Dependencies();
    }

    internal static void ConfigureUtilityServices(this IServiceCollection services)
    {
        services.Add_CommonUtils_Dependencies();
    }

    private static BotTokens PopulateBotTokens(IConfiguration config, string hostingEnvironment) => 
        hostingEnvironment switch
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

    private static string GetBotToken(IConfiguration config, string envAcronym, BotType botType)
    {
        var keyToBotToken = $"TelegramBotConfiguration:{envAcronym}-CHECKMADE-{botType}-BOT-TOKEN";
        
        return config.GetValue<string>(keyToBotToken) 
               ?? throw new InvalidOperationException($"Not found: {keyToBotToken}");
    }
}