using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Persistence.Repositories;
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
        
        services.AddScoped<ITelegramUpdateRepository, TelegramUpdateRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITelegramUserChatDestinationToRoleMapRepository, TelegramUserChatDestinationToRoleMapRepository>();
    }
}
