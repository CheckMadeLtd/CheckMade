using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Common.Utils.UiTranslation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils;

public static class DependencyRegistration
{
    public static void Add_CommonUtils_Dependencies(this IServiceCollection services)
    {
        services.AddSingleton<Randomizer>();

        services.AddSingleton<IDbOpenRetryPolicy, DbOpenRetryPolicy>();
        services.AddSingleton<IDbCommandRetryPolicy, DbCommandRetryPolicy>();
        services.AddSingleton<INetworkRetryPolicy, NetworkRetryPolicy>();

        var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));

        var config = new CommonUtilsConfig(Path.Combine(projectRoot, "UiTranslation","TargetLanguages"));
        
        services.AddSingleton<IUiTranslatorFactory, UiTranslatorFactory>(sp => 
            new UiTranslatorFactory(config.UiTranslatorTargetLanguagesPath,
                sp.GetRequiredService<ILogger<UiTranslatorFactory>>(),
                sp.GetRequiredService<ILogger<UiTranslator>>()));
    }
}