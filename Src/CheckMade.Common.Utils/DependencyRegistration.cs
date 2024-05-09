using CheckMade.Common.Interfaces.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Utils;

public static class DependencyRegistration
{
    public static void Add_CommonUtils_Dependencies(this IServiceCollection services)
    {
        services.AddSingleton<IRandomizer, Randomizer>();
    }
}