using CheckMade.Common.Utils.Generic;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Common.Utils.UiTranslation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Utils;

public static class ServiceRegistration
{
    public static void Register_CommonUtils_Services(this IServiceCollection services)
    {
        services.AddSingleton<Randomizer>();

        services.AddSingleton<IDbOpenRetryPolicy, DbOpenRetryPolicy>();
        services.AddSingleton<IDbCommandRetryPolicy, DbCommandRetryPolicy>();
        services.AddSingleton<INetworkRetryPolicy, NetworkRetryPolicy>();

        services.AddSingleton<IUiTranslatorFactory, UiTranslatorFactory>(static sp => 
            new UiTranslatorFactory(sp.GetRequiredService<ILogger<UiTranslatorFactory>>(),
                sp.GetRequiredService<ILogger<UiTranslator>>()));
    }
}