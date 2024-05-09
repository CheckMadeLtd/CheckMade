using CheckMade.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Persistence;

public static class DependencyRegistration
{
    public static void Add_CommonPersistence_Dependencies(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(dbConnectionString));
        services.AddScoped<IDbExecutionHelper>(sp =>
            new DbExecutionHelper(sp.GetRequiredService<IDbConnectionProvider>(),
                sp.GetRequiredService<ILogger<DbExecutionHelper>>()));
    }
}
