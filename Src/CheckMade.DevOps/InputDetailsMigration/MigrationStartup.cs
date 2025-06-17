using CheckMade.ChatBot.Function.Startup;
using CheckMade.Services.Persistence;
using CheckMade.DevOps.InputDetailsMigration.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.DevOps.InputDetailsMigration;

internal sealed class MigrationStartup(
    IServiceCollection services,
    IConfigurationRoot config,
    string targetEnv,
    string migIndex)
{
    public async Task StartAsync()
    {
        ConfigureDetailsMigrationServices();
        await using var sp = services.BuildServiceProvider();

        var migratorFactory = sp.GetRequiredService<MigratorByIndexFactory>();

        await migratorFactory.GetMigrator(migIndex).Match<Task>(
            async migrator =>
            {
                await (await migrator.MigrateAsync()).Match<Task>(
                    recordsUpdated => Console.Out.WriteLineAsync(
                        $"Migration '{migIndex}' succeeded, {recordsUpdated} records were updated."),
                    static failure => failure switch
                    {
                        ExceptionWrapper exw => throw exw.Exception,
                        _ => throw new InvalidOperationException($"Unexpected {nameof(BusinessError)}: " +
                                                                 $"{((BusinessError)failure).GetEnglishMessage()}")
                    });
            },
            static failure => Console.Error.WriteLineAsync(failure.GetEnglishMessage())
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

        services.RegisterCommonPersistenceServices(config, dbConnString);
        services.AddScoped<MigratorByIndexFactory>();
        services.AddScoped<MigrationRepository>();
    }
}