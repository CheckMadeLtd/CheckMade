using CheckMade.Common.ExternalServices;
using CheckMade.Common.ExternalServices.GoogleApi;
using CheckMade.Common.Persistence;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Startup;

[UsedImplicitly]
public class IntegrationTestStartup : TestStartupBase
{
    public IntegrationTestStartup()
    {
        ConfigureServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        Services.ConfigureBotClientServices(Config, HostingEnvironment);
        Services.ConfigurePersistenceServices(Config, HostingEnvironment);

        /* Here not using the usual separation of connstring and psw and then '.Replace()' because this needs to
         also work on GitHub Actions Runner / CI Environment - Integration Tests that access the production db need
         to work there too. So to keep things simple we thus store the entire connstring incl
         the real password in secrets.json on 'Development' and in a single GitHub Repo Password on 'CI' */
        var prdDbConnString = Config.GetValue<string>(DbConnectionProvider.KeyToPrdDbConnStringWithPswInEnv)
                              ?? throw new InvalidOperationException(
                                  $"Can't find {DbConnectionProvider.KeyToPrdDbConnStringWithPswInEnv}");
        
        Services.AddSingleton<PrdDbConnStringProvider>(_ => new PrdDbConnStringProvider(prdDbConnString));
        
        
        var gglApiCredentialFileName = Config.GetValue<string>(GoogleAuth.GglApiCredentialFileKey)
                                       ?? throw new InvalidOperationException(
                                           $"Can't find: {GoogleAuth.GglApiCredentialFileKey}");

        var gglApiCredentialFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            gglApiCredentialFileName);
        
        Services.Add_GoogleApi_Dependencies(gglApiCredentialFilePath);

        const string testDataGglSheetKeyInEnv = "GOOGLE_SHEET_ID_TEST_DATA";
        
        Services.AddScoped<UiSourceSheetIdProvider>(_ => new UiSourceSheetIdProvider(
            Config.GetValue<string>(testDataGglSheetKeyInEnv)
            ?? throw new InvalidOperationException(
                $"Can't find: {testDataGglSheetKeyInEnv}")));
        
        Services.Add_AzureServices_Dependencies();
    }
}
