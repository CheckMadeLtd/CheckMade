using CheckMade.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Persistence;

public static class DependencyRegistration
{
    public static void Add_CommonPersistence_Dependencies(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(dbConnectionString));
    }
}
