using CheckMade.Common.Persistence;
using CheckMade.DevOps.TlgInputDetailsMigration.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.DevOps.TlgInputDetailsMigration;

internal sealed class MigrationStartup(
    IServiceCollection services, IConfigurationRoot config,
    string targetEnv, string migIndex)
{
    public async Task StartAsync()
    {
        ConfigureDetailsMigrationServices();
        await using var sp = services.BuildServiceProvider();

        var migratorFactory = sp.GetRequiredService<MigratorByIndexFactory>();

        await migratorFactory.GetMigrator(migIndex).Match<Task>(
            async migrator =>
            {
                await (await migrator.MigrateAsync(targetEnv)).Match<Task>(
                    recordsUpdated => Console.Out.WriteLineAsync(
                        $"Migration '{migIndex}' succeeded, {recordsUpdated} records were updated."),
                    static ex => throw ex);
            },
            static error => Console.Error.WriteLineAsync(error.GetFormattedEnglish())
        );
    }

    private void ConfigureDetailsMigrationServices()
    {
        var dbConnString = targetEnv switch
        {
            "dev" => config.GetValue<string>(DbConnectionProvider.KeyToLocalDbConnStringInEnv) 
                     ?? throw new InvalidOperationException(
                         $"Can't find {DbConnectionProvider.KeyToLocalDbConnStringInEnv}"),
    
            "prd" => config.GetValue<string>(DbConnectionProvider.KeyToPrdDbConnStringWithPswInEnv) 
                     ?? throw new InvalidOperationException(
                         $"Can't find {DbConnectionProvider.KeyToPrdDbConnStringWithPswInEnv}"),
    
            _ => throw new ArgumentException($"Invalid argument for {nameof(targetEnv)}.")
        };

        services.Register_CommonPersistence_Services(dbConnString);
        services.AddScoped<MigratorByIndexFactory>();
        services.AddScoped<MigrationRepository>();
    }
}