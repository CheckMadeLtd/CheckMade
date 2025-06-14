using CheckMade.Common.DomainModel.Persistence;
using CheckMade.Common.DomainModel.Persistence.ChatBot;
using CheckMade.Common.DomainModel.Persistence.Core;
using CheckMade.Common.Persistence.Repositories.ChatBot;
using CheckMade.Common.Persistence.Repositories.Core;
using CheckMade.Common.Utils.RetryPolicies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckMade.Common.Persistence;

public static class ServiceRegistration
{
    public static void Register_CommonPersistence_Services(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(dbConnectionString));
        services.AddScoped<IDbExecutionHelper>(static sp =>
            new DbExecutionHelper(sp.GetRequiredService<IDbConnectionProvider>(),
                sp.GetRequiredService<IDbOpenRetryPolicy>(),
                sp.GetRequiredService<IDbCommandRetryPolicy>(),
                sp.GetRequiredService<ILogger<DbExecutionHelper>>()));
        
        services.AddScoped<ITlgInputsRepository, TlgInputsRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<ITlgAgentRoleBindingsRepository, TlgAgentRoleBindingsRepository>();
        services.AddScoped<ILiveEventsRepository, LiveEventsRepository>();
        services.AddScoped<IVendorsRepository, VendorsRepository>();
        services.AddScoped<IDerivedWorkflowBridgesRepository, DerivedWorkflowBridgesRepository>();
    }
}
