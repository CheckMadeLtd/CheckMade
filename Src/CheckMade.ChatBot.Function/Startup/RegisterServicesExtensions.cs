using Azure.Storage;
using Azure.Storage.Blobs;
using CheckMade.Common.Persistence;
using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.ModelFactories;
using CheckMade.ChatBot.Logic.Workflows.Global.LanguageSetting;
using CheckMade.ChatBot.Logic.Workflows.Global.LanguageSetting.States;
using CheckMade.ChatBot.Logic.Workflows.Global.Logout;
using CheckMade.ChatBot.Logic.Workflows.Global.Logout.States;
using CheckMade.ChatBot.Logic.Workflows.Global.UserAuth;
using CheckMade.ChatBot.Logic.Workflows.Global.UserAuth.States;
using CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission;
using CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.B_Details;
using CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.C_Review;
using CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.ChatBot.Telegram.BotClient;
using CheckMade.ChatBot.Telegram.Conversion;
using CheckMade.ChatBot.Telegram.Function;
using CheckMade.ChatBot.Telegram.UpdateHandling;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core.Trades;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Domain.Interfaces.ExternalServices.Utils;
using CheckMade.Common.Domain.Interfaces.Logic;
using CheckMade.Common.Domain.Interfaces.Persistence;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Domain.Logic;
using CheckMade.Common.ExternalServices.AzureServices;
using CheckMade.Common.ExternalServices.Utils;
using CheckMade.Common.Persistence.Repositories.ChatBot;
using CheckMade.Common.Persistence.Repositories.Core;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Common.Utils.UiTranslation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.ChatBot.Function.Startup;

public static class RegisterServicesExtensions
{
    // Division of ChatBotTelegram Services into Function and Business needed due to separate treatment in Tests
    internal static void RegisterChatBotTelegramFunctionServices(
        this IServiceCollection services, IConfiguration config, string hostingEnvironment)
    {
        // Function
        services.AddScoped<IBotFunction, TelegramBotFunction>();
        
        // BotClients
        services.AddSingleton<IBotClientFactory, BotClientFactory>();

        var interactionModes = Enum.GetNames(typeof(InteractionMode));
        foreach (var mode in interactionModes)
        {
            services.AddHttpClient($"CheckMade{mode}Bot");            
        }    
        
        services.AddSingleton<BotTokens>(_ => PopulateBotTokens(config, hostingEnvironment));
    }

    internal static void RegisterChatBotTelegramBusinessServices(this IServiceCollection services)
    {
        // UpdateHandling
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<IBotUpdateSwitch, BotUpdateSwitch>();
        services.AddSingleton<ILastOutputMessageIdCache, LastOutputTelegramMessageIdCache>();

        // Conversion
        services.AddScoped<IToModelConverterFactory, ToModelConverterFactory>();
        services.AddScoped<IOutputToReplyMarkupConverterFactory, OutputToReplyMarkupConverterFactory>();
    }
    
    public static void RegisterCommonPersistenceServices(
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
        
        services.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(dbConnectionString));
        
        services.AddScoped<IDbExecutionHelper>(static sp =>
            new DbExecutionHelper(sp.GetRequiredService<IDbConnectionProvider>(),
                sp.GetRequiredService<IDbOpenRetryPolicy>(),
                sp.GetRequiredService<IDbCommandRetryPolicy>(),
                sp.GetRequiredService<ILogger<DbExecutionHelper>>()));
        
        services.AddScoped<IInputsRepository, InputsRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IAgentRoleBindingsRepository, AgentRoleBindingsRepository>();
        services.AddScoped<ILiveEventsRepository, LiveEventsRepository>();
        services.AddScoped<IVendorsRepository, VendorsRepository>();
        services.AddScoped<IDerivedWorkflowBridgesRepository, DerivedWorkflowBridgesRepository>();
    }

    internal static void RegisterCommonBusinessLogicServices(this IServiceCollection services)
    {
        services.AddScoped<IStakeholderReporter<SanitaryTrade>, StakeholderReporter<SanitaryTrade>>();
        services.AddScoped<IStakeholderReporter<SiteCleanTrade>, StakeholderReporter<SiteCleanTrade>>();
    }
    
    public static void RegisterCommonUtilsServices(this IServiceCollection services)
    {
        services.AddSingleton<Randomizer>();

        services.AddSingleton<IDbOpenRetryPolicy, DbOpenRetryPolicy>();
        services.AddSingleton<IDbCommandRetryPolicy, DbCommandRetryPolicy>();
        services.AddSingleton<INetworkRetryPolicy, NetworkRetryPolicy>();

        services.AddSingleton<IUiTranslatorFactory, UiTranslatorFactory>(static sp => 
            new UiTranslatorFactory(sp.GetRequiredService<ILogger<UiTranslatorFactory>>(),
                sp.GetRequiredService<ILogger<UiTranslator>>()));
    }

    internal static void RegisterCommonExternalServices(this IServiceCollection services, IConfiguration config)
    {
        // Azure Blob Service
        
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
        
        services.AddScoped<IBlobLoader, BlobLoader>();
        services.AddScoped<BlobContainerClient>(_ =>
            new BlobContainerClient(
                new Uri(blobContainerUriKey),
                new StorageSharedKeyCredential(blobContainerAccountName, blobContainerAccountKey)));

        // Other
        
        services.AddHttpClient();
        services.AddSingleton<IHttpDownloader, HttpDownloader>();
    }
    
    internal static void RegisterChatBotLogicServices(this IServiceCollection services)
    {
        services.AddSingleton<IDomainGlossary, DomainGlossary>();
        
        services.AddScoped<IInputProcessor, InputProcessor>();
        services.AddScoped<IWorkflowIdentifier, WorkflowIdentifier>();
        services.AddScoped<IGeneralWorkflowUtils, GeneralWorkflowUtils>();
        services.AddScoped<IStateMediator, StateMediator>();

        services.AddScoped<ISubmissionFactory<SanitaryTrade>, SubmissionFactory<SanitaryTrade>>();
        services.AddScoped<ISubmissionFactory<SiteCleanTrade>, SubmissionFactory<SiteCleanTrade>>();

        services.AddScoped<UserAuthWorkflow>();
        services.AddScoped<NewSubmissionWorkflow>();
        services.AddScoped<LanguageSettingWorkflow>();
        services.AddScoped<LogoutWorkflow>();
        
        services.AddScoped<ILanguageSettingSelect, LanguageSettingSelect>();
        services.AddScoped<ILanguageSettingSet, LanguageSettingSet>();

        services.AddScoped<ILogoutWorkflowConfirm, LogoutWorkflowConfirm>();
        services.AddScoped<ILogoutWorkflowLoggedOut, LogoutWorkflowLoggedOut>();
        services.AddScoped<ILogoutWorkflowAborted, LogoutWorkflowAborted>();

        services.AddScoped<IUserAuthWorkflowTokenEntry, UserAuthWorkflowTokenEntry>();
        services.AddScoped<IUserAuthWorkflowAuthenticated, UserAuthWorkflowAuthenticated>();

        services.AddScoped<INewSubmissionTradeSelection, NewSubmissionTradeSelection>();
        services.AddScoped<INewSubmissionCancelConfirmation<SanitaryTrade>, NewSubmissionCancelConfirmation<SanitaryTrade>>();
        services.AddScoped<INewSubmissionCancelConfirmation<SiteCleanTrade>, NewSubmissionCancelConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionCancelled<SanitaryTrade>, NewSubmissionCancelled<SanitaryTrade>>();
        services.AddScoped<INewSubmissionCancelled<SiteCleanTrade>, NewSubmissionCancelled<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionConsumablesSelection<SanitaryTrade>, NewSubmissionConsumablesSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionConsumablesSelection<SiteCleanTrade>, NewSubmissionConsumablesSelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionEditMenu<SanitaryTrade>, NewSubmissionEditMenu<SanitaryTrade>>();
        services.AddScoped<INewSubmissionEditMenu<SiteCleanTrade>, NewSubmissionEditMenu<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionEvidenceEntry<SanitaryTrade>, NewSubmissionEvidenceEntry<SanitaryTrade>>();
        services.AddScoped<INewSubmissionEvidenceEntry<SiteCleanTrade>, NewSubmissionEvidenceEntry<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionFacilitySelection<SanitaryTrade>, NewSubmissionFacilitySelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionFacilitySelection<SiteCleanTrade>, NewSubmissionFacilitySelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionAssessmentRating<SanitaryTrade>, NewSubmissionAssessmentRating<SanitaryTrade>>();
        services.AddScoped<INewSubmissionAssessmentRating<SiteCleanTrade>, NewSubmissionAssessmentRating<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionReview<SanitaryTrade>, NewSubmissionReview<SanitaryTrade>>();
        services.AddScoped<INewSubmissionReview<SiteCleanTrade>, NewSubmissionReview<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSphereConfirmation<SanitaryTrade>, NewSubmissionSphereConfirmation<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSphereConfirmation<SiteCleanTrade>, NewSubmissionSphereConfirmation<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSphereSelection<SanitaryTrade>, NewSubmissionSphereSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSphereSelection<SiteCleanTrade>, NewSubmissionSphereSelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionTypeSelection<SanitaryTrade>, NewSubmissionTypeSelection<SanitaryTrade>>();
        services.AddScoped<INewSubmissionTypeSelection<SiteCleanTrade>, NewSubmissionTypeSelection<SiteCleanTrade>>();
        services.AddScoped<INewSubmissionSucceeded<SanitaryTrade>, NewSubmissionSucceeded<SanitaryTrade>>();
        services.AddScoped<INewSubmissionSucceeded<SiteCleanTrade>, NewSubmissionSucceeded<SiteCleanTrade>>();
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