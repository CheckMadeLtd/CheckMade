using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Persistence.Repositories.Core;
using CheckMade.Common.Persistence.Repositories.Tlg;
using CheckMade.Common.Utils.RetryPolicies;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Persistence;

public static class ServiceRegistration
{
    public static void Register_CommonPersistence_Services(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(dbConnectionString));
        services.AddScoped<IDbExecutionHelper>(sp =>
            new DbExecutionHelper(sp.GetRequiredService<IDbConnectionProvider>(),
                sp.GetRequiredService<IDbOpenRetryPolicy>(),
                sp.GetRequiredService<IDbCommandRetryPolicy>()));
        
        services.AddScoped<ITlgInputRepository, TlgInputRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITlgClientPortToRoleMapRepository, TlgClientPortToRoleMapRepository>();
    }
}
