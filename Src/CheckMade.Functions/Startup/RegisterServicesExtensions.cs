using Azure.Storage;
using Azure.Storage.Blobs;
using CheckMade.Abstract.Domain.Data.Bot.UserInteraction;
using CheckMade.Services.Persistence;
using CheckMade.Bot.Workflows;
using CheckMade.Bot.Workflows.ModelFactories;
using CheckMade.Bot.Telegram.BotClient;
using CheckMade.Bot.Telegram.Conversion;
using CheckMade.Bot.Telegram.Function;
using CheckMade.Bot.Telegram.UpdateHandling;
using CheckMade.Abstract.Domain.Data.Core.Trades;
using CheckMade.Abstract.Domain.Interfaces.Bot.Function;
using CheckMade.Abstract.Domain.Interfaces.Bot.Logic;
using CheckMade.Abstract.Domain.Interfaces.ExternalServices.AzureServices;
using CheckMade.Abstract.Domain.Interfaces.ExternalServices.Utils;
using CheckMade.Abstract.Domain.Interfaces.Logic;
using CheckMade.Abstract.Domain.Interfaces.Persistence;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Bot;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Core;
using CheckMade.Bot.Workflows.Global.LanguageSetting;
using CheckMade.Bot.Workflows.Global.LanguageSetting.States;
using CheckMade.Bot.Workflows.Global.Logout;
using CheckMade.Bot.Workflows.Global.Logout.States;
using CheckMade.Bot.Workflows.Global.UserAuth;
using CheckMade.Bot.Workflows.Global.UserAuth.States;
using CheckMade.Bot.Workflows.Ops.NewSubmission;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.A_Init;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.C_Review;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.D_Terminators;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Services.ExtAPIs.AzureServices;
using CheckMade.Services.ExtAPIs.Utils;
using CheckMade.Services.Logic;
using CheckMade.Services.Persistence.Repositories.Bot;
using CheckMade.Services.Persistence.Repositories.Core;
using General.Utils.RetryPolicies;
using General.Utils.UiTranslation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Functions.Startup;

public static class RegisterServicesExtensions
{
    // Division of BotTelegram Services into Function and Business needed due to separate treatment in Tests
    internal static void RegisterBotTelegramFunctionServices(
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

    internal static void RegisterBotTelegramHandlingServices(this IServiceCollection services)
    {
        // UpdateHandling
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<IBotUpdateSwitch, BotUpdateSwitch>();
        services.AddSingleton<ILastOutputMessageIdCache, LastOutputTelegramMessageIdCache>();

        // Conversion
        services.AddScoped<IToModelConverterFactory, ToModelConverterFactory>();
        services.AddScoped<IOutputToReplyMarkupConverterFactory, OutputToReplyMarkupConverterFactory>();
    }
    
    public static void RegisterServicesPersistence(
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

    internal static void RegisterServicesLogic(this IServiceCollection services)
    {
        services.AddScoped<IStakeholderReporter<SanitaryTrade>, StakeholderReporter<SanitaryTrade>>();
        services.AddScoped<IStakeholderReporter<SiteCleanTrade>, StakeholderReporter<SiteCleanTrade>>();
    }
    
    public static void RegisterGeneralUtils(this IServiceCollection services)
    {
        services.AddSingleton<Randomizer>();

        services.AddSingleton<IDbOpenRetryPolicy, DbOpenRetryPolicy>();
        services.AddSingleton<IDbCommandRetryPolicy, DbCommandRetryPolicy>();
        services.AddSingleton<INetworkRetryPolicy, NetworkRetryPolicy>();

        services.AddSingleton<IUiTranslatorFactory, UiTranslatorFactory>(static sp => 
            new UiTranslatorFactory(sp.GetRequiredService<ILogger<UiTranslatorFactory>>(),
                sp.GetRequiredService<ILogger<UiTranslator>>()));
    }

    // ReSharper disable once InconsistentNaming
    internal static void RegisterServicesExtAPIs(this IServiceCollection services, IConfiguration config)
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
    
    internal static void RegisterBotLogicServices(this IServiceCollection services)
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