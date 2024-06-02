using CheckMade.Common.Persistence;
using CheckMade.DevOps.DetailsMigration.TelegramUpdates.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

namespace CheckMade.DevOps.DetailsMigration.TelegramUpdates;

internal class MigrationStartup(
    IServiceCollection services, IConfigurationRoot config,
    string targetEnv, string migIndex)
{
    public async Task StartAsync()
    {
        ConfigureDetailsMigrationServices();
        await using var sp = services.BuildServiceProvider();

        // var logger = sp.GetRequiredService<ILogger<MigrationStartup>>();
        var migratorFactory = sp.GetRequiredService<MigratorByIndexFactory>();

        await migratorFactory.GetMigrator(migIndex).Match<Task>(
            async migrator =>
            {
                await (await migrator.MigrateAsync(targetEnv)).Match<Task>(
                    recordsUpdated => Console.Out.WriteLineAsync(
                        $"Migration '{migIndex}' succeeded, {recordsUpdated} records were updated."),
                    error =>
                    {
                        // ReSharper disable once ConvertToLambdaExpression
                        throw error.Exception ?? new Exception(error.FailureMessage!.GetFormattedEnglish());
                        // logger.LogError(ex.Message, ex.StackTrace);
                        // return Console.Error.WriteLineAsync(ex.Message);
                    });
            },
            errorMessage => Console.Error.WriteLineAsync(errorMessage.FailureMessage!.GetFormattedEnglish())
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

        services.Add_CommonPersistence_Dependencies(dbConnString);
        services.AddScoped<MigratorByIndexFactory>();
        services.AddScoped<MigrationRepository>();
    }
}