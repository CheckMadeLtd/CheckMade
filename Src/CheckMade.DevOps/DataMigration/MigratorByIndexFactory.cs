using System.Reflection;
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.DevOps.DataMigration.Repositories;

namespace CheckMade.DevOps.DataMigration;

internal class MigratorByIndexFactory
{
    private readonly Dictionary<string, DataMigratorBase> _migratorByIndex;
    
    public MigratorByIndexFactory(MessagesMigrationRepository migRepo)
    {
        _migratorByIndex = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => string.IsNullOrEmpty(type.Namespace) == false &&
                           type.Namespace.StartsWith("CheckMade.DevOps.DataMigration.Migrators") &&
                           typeof(DataMigratorBase).IsAssignableFrom(type))
            .ToDictionary(
                type => GetMigratorIndexFromTypeName(type.Name),
                type => (DataMigratorBase)(Activator.CreateInstance(type, migRepo) 
                                        ?? throw new InvalidOperationException(
                                            $"Could not create instance for {type.FullName}"))
            );
    }
    
    private static string GetMigratorIndexFromTypeName(string typeName)
    {
        // Relies on naming convention 'MigXXXX'
        return typeName.Substring(3, 4);
    }

    public Result<DataMigratorBase> GetMigrator(string migIndex) => 
        _migratorByIndex.TryGetValue(migIndex, out var migrator) switch
        {
            true => Result<DataMigratorBase>.FromSuccess(migrator),
            false => Result<DataMigratorBase>.FromError($"No migrator called 'Mig{migIndex}' was found.")
        };
}
