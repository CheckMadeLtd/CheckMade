using CheckMade.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Persistence;

public static class DependencyRegistration
{
    public static void Add_Persistence_Dependencies(this IServiceCollection services, string dbConnectionString)
    {
        services.AddSingleton<IDbConnectionProvider>(_ => new DbConnectionProvider(dbConnectionString));
        services.AddTransient<ITelegramMessageRepo, TelegramMessageRepo>();
    }
}
