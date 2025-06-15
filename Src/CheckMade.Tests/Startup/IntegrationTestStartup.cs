using CheckMade.Common.ExternalServices.GoogleApi;
using CheckMade.Common.Persistence;
using CheckMade.ChatBot.Function.Startup;
using CheckMade.Common.Domain.Interfaces.ExternalServices.GoogleApi;
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
        RegisterServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        Services.RegisterChatBotTelegramFunctionServices(Config, HostingEnvironment);
        Services.RegisterCommonPersistenceServices(Config, HostingEnvironment);
        Services.RegisterCommonExternalServices(Config);

        /* Here not using the usual separation of connstring and psw and then '.Replace()' because this needs to
         also work on GitHub Actions Runner / CI Environment - Integration Tests that access the production db need
         to work there too. So to keep things simple we thus store the entire connstring incl
         the real password in secrets.json on 'Development' and in a single GitHub Repo Password on 'CI' */
        var prdDbConnString = Config.GetValue<string>(DbConnectionProvider.KeyToPrdDbConnStringWithPswInEnv)
                              ?? throw new InvalidOperationException(
                                  $"Can't find {DbConnectionProvider.KeyToPrdDbConnStringWithPswInEnv}");
        
        Services.AddSingleton<PrdDbConnStringProvider>(_ => new PrdDbConnStringProvider(prdDbConnString));
        
        RegisterGoogleApiServices();
    }

    private void RegisterGoogleApiServices()
    {
        var gglApiCredentialFileName = Config.GetValue<string>(GoogleAuth.GglApiCredentialFileKey)
                                       ?? throw new InvalidOperationException(
                                           $"Can't find: {GoogleAuth.GglApiCredentialFileKey}");

        var gglApiCredentialFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            gglApiCredentialFileName);
        
        Services.AddScoped<GoogleAuth>(_ => new GoogleAuth(gglApiCredentialFilePath));
        Services.AddScoped<ISheetsService, GoogleSheetsService>();
        
        const string testDataGglSheetKeyInEnv = "GOOGLE_SHEET_ID_TEST_DATA";
        
        Services.AddScoped<TestDataSheetIdProvider>(_ => new TestDataSheetIdProvider(
            Config.GetValue<string>(testDataGglSheetKeyInEnv)
            ?? throw new InvalidOperationException(
                $"Can't find: {testDataGglSheetKeyInEnv}")));
    }
}
