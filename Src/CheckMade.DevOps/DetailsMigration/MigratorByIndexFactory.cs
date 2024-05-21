using System.Reflection;
using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.DevOps.DetailsMigration.Repositories.Messages;

namespace CheckMade.DevOps.DetailsMigration;

internal class MigratorByIndexFactory
{
    private readonly Dictionary<string, DetailsMigratorBase> _migratorByIndex;
    
    public MigratorByIndexFactory(MigrationRepository migRepo)
    {
        _migratorByIndex = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => string.IsNullOrEmpty(type.Namespace) == false &&
                           type.Namespace.StartsWith("CheckMade.DevOps.DetailsMigration.Migrators") &&
                           typeof(DetailsMigratorBase).IsAssignableFrom(type))
            .ToDictionary(
                type => GetMigratorIndexFromTypeName(type.Name),
                type => (DetailsMigratorBase)(Activator.CreateInstance(type, migRepo) 
                                        ?? throw new InvalidOperationException(
                                            $"Could not create instance for {type.FullName}"))
            );
    }
    
    private static string GetMigratorIndexFromTypeName(string typeName)
    {
        // Relies on naming convention 'MigXXXX'
        return typeName.Substring(3, 4);
    }

    public Result<DetailsMigratorBase> GetMigrator(string migIndex) => 
        _migratorByIndex.TryGetValue(migIndex, out var migrator) switch
        {
            true => Result<DetailsMigratorBase>.FromSuccess(migrator),
            false => Result<DetailsMigratorBase>.FromError($"No migrator called 'Mig{migIndex}' was found.")
        };
}
