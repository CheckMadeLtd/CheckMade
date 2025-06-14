using CheckMade.Common.Persistence;
using CheckMade.Common.Utils;
using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Telegram;
using CheckMade.ChatBot.Telegram.BotClient;
using CheckMade.Common.DomainModel;
using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using CheckMade.Common.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Function.Startup;

internal static class RegisterServicesExtensions
{
    internal static void RegisterChatBotTelegramServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        services.RegisterChatBotTelegramFunctionServices();
        services.RegisterChatBotTelegramUpdateHandlingServices();
        services.RegisterChatBotTelegramConversionServices();
        services.RegisterChatBotTelegramBotClientServices();
        
        services.AddSingleton<BotTokens>(_ => PopulateBotTokens(config, hostingEnvironment));
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
            
            _ => throw new ArgumentException(nameof(hostingEnvironment))
        };
        
        services.Register_CommonPersistence_Services(dbConnectionString);
    }

    internal static void RegisterChatBotLogicServices(this IServiceCollection services)
    {
        services.Register_ChatBotLogic_Services();
    }

    internal static void RegisterCommonBusinessLogicServices(this IServiceCollection services)
    {
        services.Register_CommonBusinessLogic_Services();
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
                GetBotToken(config, "DEV", InteractionMode.Operations),
                GetBotToken(config, "DEV", InteractionMode.Communications),
                GetBotToken(config, "DEV", InteractionMode.Notifications)),

            "Staging" or "CI" => new BotTokens(
                GetBotToken(config, "STG", InteractionMode.Operations),
                GetBotToken(config, "STG", InteractionMode.Communications),
                GetBotToken(config, "STG", InteractionMode.Notifications)),

            "Production" => new BotTokens(
                GetBotToken(config, "PRD", InteractionMode.Operations),
                GetBotToken(config, "PRD", InteractionMode.Communications),
                GetBotToken(config, "PRD", InteractionMode.Notifications)),

            _ => throw new ArgumentException(nameof(hostingEnvironment))
        };

    private static string GetBotToken(IConfiguration config, string envAcronym, InteractionMode interactionMode)
    {
        var keyToBotToken = $"TelegramBotConfiguration:{envAcronym}-CHECKMADE-{interactionMode}-BOT-TOKEN";
        
        return config.GetValue<string>(keyToBotToken) 
               ?? throw new InvalidOperationException($"Not found: {keyToBotToken}");
    }
}