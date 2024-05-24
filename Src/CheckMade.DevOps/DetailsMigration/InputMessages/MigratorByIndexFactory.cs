using System.Reflection;
using CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

namespace CheckMade.DevOps.DetailsMigration.InputMessages;

internal class MigratorByIndexFactory
{
    private readonly Dictionary<string, MigratorBase> _migratorByIndex;
    
    public MigratorByIndexFactory(MigrationRepository migRepo)
    {
        _migratorByIndex = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => string.IsNullOrEmpty(type.Namespace) == false &&
                           type.Namespace.StartsWith("CheckMade.DevOps.DetailsMigration.InputMessages.Migrators") &&
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
            false => Result<MigratorBase>.FromError(Ui("No migrator called 'Mig{0}' was found.", migIndex))
        };
}
