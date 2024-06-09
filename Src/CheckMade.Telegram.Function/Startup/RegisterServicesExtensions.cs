using CheckMade.Common.ExternalServices;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Common.Persistence;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversion;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Function.Startup;

internal static class RegisterServicesExtensions
{
    internal static void RegisterTelegramFunctionBotClientServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        services.AddSingleton<IBotClientFactory, BotClientFactory>();
        services.AddSingleton<BotTokens>(_ => PopulateBotTokens(config, hostingEnvironment));

        var botTypes = Enum.GetNames(typeof(TlgBotType));
        foreach (var botType in botTypes)
        {
            services.AddHttpClient($"CheckMade{botType}Bot");            
        }    
    }

    internal static void RegisterTelegramFunctionUpdateHandlingServices(this IServiceCollection services)
    {
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<IBotUpdateSwitch, BotUpdateSwitch>();
    }
    
    internal static void RegisterCommonPersistenceServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
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
        
        services.Register_CommonPersistence_Services(dbConnectionString);
    }

    internal static void RegisterTelegramFunctionConversionServices(this IServiceCollection services)
    {
        services.AddSingleton<IToModelConverterFactory, ToModelConverterFactory>();
        services.AddScoped<IOutputToReplyMarkupConverterFactory, OutputToReplyMarkupConverterFactory>();
    }

    internal static void RegisterTelegramLogicServices(this IServiceCollection services)
    {
        services.Register_TelegramLogic_Services();
    }
    
    internal static void RegisterCommonUtilsServices(this IServiceCollection services)
    {
        services.Register_CommonUtils_Services();
    }

    internal static void RegisterCommonExternalServices(this IServiceCollection services, IConfiguration config)
    {
        // This style of spelling of keys so they work both, in UNIX env on GitHub Actions and in Azure Keyvault!
        const string keyToBlobContainerUri = "BlobContainerClientUri";
        const string keyToBlobContainerAccountName = "BlobContainerClientAccountName";
        const string keyToBlobContainerAccountKey = "BlobContainerClientAccountKey";

        var blobContainerUriKey = config.GetValue<string>(keyToBlobContainerUri)
                                  ?? throw new InvalidOperationException($"Can't find {keyToBlobContainerUri}");

        var blobContainerAccountName = config.GetValue<string>(keyToBlobContainerAccountName)
                                       ?? throw new InvalidOperationException(
                                           $"Can't find {keyToBlobContainerAccountName}");

        var blobContainerAccountKey = config.GetValue<string>(keyToBlobContainerAccountKey)
                                      ?? throw new InvalidOperationException(
                                          $"Can't find {keyToBlobContainerAccountKey}");

        services.Register_AzureServices_Services(
            blobContainerUriKey, blobContainerAccountName, blobContainerAccountKey);
        
        services.Register_OtherExternalFacingServices_Services();
    }

    private static BotTokens PopulateBotTokens(IConfiguration config, string hostingEnvironment) => 
        hostingEnvironment switch
        {
            "Development" => new BotTokens(
                GetBotToken(config, "DEV", TlgBotType.Operations),
                GetBotToken(config, "DEV", TlgBotType.Communications),
                GetBotToken(config, "DEV", TlgBotType.Notifications)),

            "Staging" or "CI" => new BotTokens(
                GetBotToken(config, "STG", TlgBotType.Operations),
                GetBotToken(config, "STG", TlgBotType.Communications),
                GetBotToken(config, "STG", TlgBotType.Notifications)),

            "Production" => new BotTokens(
                GetBotToken(config, "PRD", TlgBotType.Operations),
                GetBotToken(config, "PRD", TlgBotType.Communications),
                GetBotToken(config, "PRD", TlgBotType.Notifications)),

            _ => throw new ArgumentException((nameof(hostingEnvironment)))
        };

    private static string GetBotToken(IConfiguration config, string envAcronym, TlgBotType botType)
    {
        var keyToBotToken = $"TelegramBotConfiguration:{envAcronym}-CHECKMADE-{botType}-BOT-TOKEN";
        
        return config.GetValue<string>(keyToBotToken) 
               ?? throw new InvalidOperationException($"Not found: {keyToBotToken}");
    }
}