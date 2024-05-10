using CheckMade.Common.Interfaces.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Utils;

public static class DependencyRegistration
{
    public static void Add_CommonUtils_Dependencies(this IServiceCollection services)
    {
        services.AddSingleton<IRandomizer, Randomizer>();

        services.AddSingleton<IDbRetryPolicy, DbRetryPolicy>();
        services.AddSingleton<INetworkRetryPolicy, NetworkRetryPolicy>();
    }
}