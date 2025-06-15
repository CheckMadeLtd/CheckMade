using System.Reflection;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.DevOps.InputDetailsMigration.Helpers;

namespace CheckMade.DevOps.InputDetailsMigration;

internal sealed class MigratorByIndexFactory
{
    private readonly IDictionary<string, MigratorBase> _migratorByIndex;
    
    public MigratorByIndexFactory(MigrationRepository migRepo)
    {
        _migratorByIndex = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(static type => string.IsNullOrEmpty(type.Namespace) == false &&
                                  type.Namespace.StartsWith("CheckMade.DevOps.InputDetailsMigration.Migrators") &&
                                  typeof(MigratorBase).IsAssignableFrom(type))
            .ToDictionary(
                static type => GetMigratorIndexFromTypeName(type.Name),
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
