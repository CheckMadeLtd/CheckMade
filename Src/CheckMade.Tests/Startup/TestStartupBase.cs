using System.Configuration;
using CheckMade.Function.Startup;
using CheckMade.ChatBot.Telegram.UpdateHandling;
using General.Utils.UiTranslation;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Tests.Startup;

public abstract class TestStartupBase
{
    protected IConfigurationRoot Config { get; private init; }
    internal string HostingEnvironment { get; private init; }
    internal ServiceCollection Services { get; } = [];
    
    protected TestStartupBase()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
        
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            // If this file can't be found, we assume the test runs on GitHub Actions Runner with corresp. env. variables! 
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            // This config (the secrets.json of the main ChatBot project) gets ignored on the GitHub Actions Runner
            .AddUserSecrets("dd4f1069-ae94-4987-9751-690e8da6f3c0") 
            // This also includes Env Vars set in GitHub Actions Workflow
            .AddEnvironmentVariables();
        Config = configBuilder.Build();

        // This is taken either from local.settings.json or from env variable set in GitHub Actions workflow!
        const string keyToHostEnv = "HOSTING_ENVIRONMENT";
        HostingEnvironment = Config.GetValue<string>(keyToHostEnv)
                             ?? throw new ConfigurationErrorsException($"Can't find {keyToHostEnv}");
    }

    protected void RegisterServices()
    {
        RegisterBaseServices();
        RegisterTestTypeSpecificServices();
    }

    private void RegisterBaseServices()
    {
        Services.AddLogging(static loggingConfig =>
        {
            loggingConfig.ClearProviders();
            loggingConfig.AddConsole(); 
            loggingConfig.AddDebug(); 
        });
        
        Services.AddSingleton<ITelegramUpdateGenerator, TelegramUpdateGenerator>();
        Services.AddSingleton<IInputGenerator, InputGenerator>();
        
        
        Services.AddScoped<DefaultUiLanguageCodeProvider>(static _ => new DefaultUiLanguageCodeProvider(LanguageCode.en));

        Services.RegisterChatBotTelegramBusinessServices();
        Services.RegisterChatBotLogicServices();
        
        Services.RegisterCommonBusinessLogicServices();
        Services.RegisterCommonUtilsServices();
    }

    protected abstract void RegisterTestTypeSpecificServices();
}