using CheckMade.Common.ExternalServices;
using CheckMade.Common.ExternalServices.GoogleApi;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Tests.Startup;

[UsedImplicitly]
public class IntegrationTestStartup : TestStartupBase
{
    public IntegrationTestStartup()
    {
        ConfigureServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        var gglApiCredentialFileName = Config.GetValue<string>(GoogleAuth.GglApiCredentialFileKey)
                               ?? throw new InvalidOperationException(
                                   $"Can't find: {GoogleAuth.GglApiCredentialFileKey}");

        var gglApiCredentialFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            gglApiCredentialFileName);
        
        Services.Add_GoogleApi_Dependencies(gglApiCredentialFilePath);

        const string uiSourceGglSheetKeyInEnv = "GOOGLE_SHEET_ID_UI_SOURCE";
        
        Services.AddScoped<UiSourceSheetIdProvider>(_ => new UiSourceSheetIdProvider(
            Config.GetValue<string>(uiSourceGglSheetKeyInEnv)
            ?? throw new InvalidOperationException(
                $"Can't find: {uiSourceGglSheetKeyInEnv}")));
    }
}