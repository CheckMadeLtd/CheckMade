using CheckMade.Common.Persistence;
using CheckMade.Telegram.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.DevOps.DataMigration;

internal class DataMigrationStartup(
    IServiceCollection services, IConfigurationRoot config,
    string targetEnv, string migIndex)
{
    public async Task StartAsync()
    {
        ConfigureDataMigrationServices();
        
        
        // ToDo: This contains a nested call to two subsequent Result<T> operations ==> make more concise with LINQ!
        await using (var sp = services.BuildServiceProvider())
        {
            var migratorFactory = sp.GetRequiredService<MigratorByIndexFactory>();

            await migratorFactory.GetMigrator(migIndex).Match<Task>(
                
                async migrator =>
                {
                    var migrationOutcome = await migrator.MigrateAsync(targetEnv);
                    
                    await migrationOutcome.Match<Task>(
                        
                        recordsUpdated => Console.Out.WriteLineAsync(
                            $"Migration '{migIndex}' succeeded, {recordsUpdated} records were updated."),
                        
                        errorMessage => Console.Error.WriteLineAsync(errorMessage)
                    );
                },
                errorMessage => Console.Error.WriteLineAsync(errorMessage)
            );
        }
    }

    private void ConfigureDataMigrationServices()
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
        services.Add_TelegramPersistence_Dependencies();

        services.AddScoped<MigratorByIndexFactory>();
    }
}