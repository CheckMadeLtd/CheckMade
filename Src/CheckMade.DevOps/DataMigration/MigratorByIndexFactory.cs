using System.Reflection;
using CheckMade.Common.Utils;

namespace CheckMade.DevOps.DataMigration;

internal class MigratorByIndexFactory
{
    private readonly Dictionary<string, IDataMigrator> _migratorByIndex;
    
    public MigratorByIndexFactory()
    {
        _migratorByIndex = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => string.IsNullOrEmpty(type.Namespace) == false &&
                           type.Namespace.StartsWith("CheckMade.DevOps.DataMigration.Migrators") &&
                           typeof(IDataMigrator).IsAssignableFrom(type))
            .ToDictionary(
                type => GetMigratorIndexFromTypeName(type.Name),
                type => (IDataMigrator)(Activator.CreateInstance(type) 
                                        ?? throw new InvalidOperationException(
                                            $"Could not create instance for {type.FullName}"))
            );
    }
    
    private static string GetMigratorIndexFromTypeName(string typeName)
    {
        // Relies on naming convention 'MigXXXX'
        return typeName.Substring(3, 4);
    }

    public Result<IDataMigrator> GetMigrator(string migIndex) => 
        _migratorByIndex.TryGetValue(migIndex, out var migrator) switch
        {
            true => new Result<IDataMigrator>(migrator),
            false => $"No migrator called 'Mig{migIndex}' was found."
        };
}
