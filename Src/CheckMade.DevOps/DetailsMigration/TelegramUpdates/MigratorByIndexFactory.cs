using System.Reflection;
using CheckMade.DevOps.DetailsMigration.TelegramUpdates.Helpers;

namespace CheckMade.DevOps.DetailsMigration.TelegramUpdates;

internal class MigratorByIndexFactory
{
    private readonly IDictionary<string, MigratorBase> _migratorByIndex;
    
    public MigratorByIndexFactory(MigrationRepository migRepo)
    {
        _migratorByIndex = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => string.IsNullOrEmpty(type.Namespace) == false &&
                           type.Namespace.StartsWith("CheckMade.DevOps.DetailsMigration.Updates.Migrators") &&
                           typeof(MigratorBase).IsAssignableFrom(type))
            .ToDictionary(
                type => GetMigratorIndexFromTypeName(type.Name),
                type => (MigratorBase)(Activator.CreateInstance(type, migRepo) 
                                        ?? throw new InvalidOperationException(
                                            $"Could not create instance for {type.FullName}"))
            );
    }
    
    private static string GetMigratorIndexFromTypeName(string typeName)
    {
        // Relies on naming convention 'MigXXXX'
        return typeName.Substring(3, 4);
    }

    public Result<MigratorBase> GetMigrator(string migIndex) => 
        _migratorByIndex.TryGetValue(migIndex, out var migrator) switch
        {
            true => migrator,
            false => Ui("No migrator called 'Mig{0}' was found.", migIndex)
        };
}
