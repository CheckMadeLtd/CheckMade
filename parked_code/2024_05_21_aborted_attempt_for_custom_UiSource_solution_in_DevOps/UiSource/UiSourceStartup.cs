using CheckMade.Common.ExternalServices;
using CheckMade.Common.ExternalServices.GoogleApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.DevOps.UiSource;

public class UiSourceStartup(IServiceCollection services, IConfigurationRoot config)
{
    public async Task StartAsync()
    {
        ConfigureUiSourceServices();
    }

    private void ConfigureUiSourceServices()
    {
        var gglApiCredentialFileName = config.GetValue<string>(GoogleAuth.GglApiCredentialFileKey)
                                       ?? throw new InvalidOperationException(
                                           $"Can't find: {GoogleAuth.GglApiCredentialFileKey}");
        
        var gglApiCredentialFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            gglApiCredentialFileName);
        
        services.Add_GoogleApi_Dependencies(gglApiCredentialFilePath);
        
        const string uiSourceGglSheetKeyInEnv = "GOOGLE_SHEET_ID_UI_SOURCE";
            
        services.AddScoped<UiSourceSheetIdProvider>(_ => new UiSourceSheetIdProvider(
            config.GetValue<string>(uiSourceGglSheetKeyInEnv)
        ?? throw new InvalidOperationException(
            $"Can't find: {uiSourceGglSheetKeyInEnv}")));

        services.AddScoped<UiSourceSheetData>();
        services.AddScoped<UiStringsSheetDataToJsonConverter>();
    }
}