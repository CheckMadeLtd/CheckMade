using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Common.Utils.UiTranslation;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Utils;

public static class DependencyRegistration
{
    public static void Add_CommonUtils_Dependencies(this IServiceCollection services)
    {
        services.AddSingleton<Randomizer>();

        services.AddSingleton<IDbOpenRetryPolicy, DbOpenRetryPolicy>();
        services.AddSingleton<IDbCommandRetryPolicy, DbCommandRetryPolicy>();
        services.AddSingleton<INetworkRetryPolicy, NetworkRetryPolicy>();

        services.AddSingleton<IUiTranslatorFactory, UiTranslatorFactory>();
    }
}