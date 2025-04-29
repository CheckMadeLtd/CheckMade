using System.Reflection;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

namespace CheckMade.DevOps.TlgInputDetailsMigration;

internal sealed class MigratorByIndexFactory
{
    private readonly IDictionary<string, MigratorBase> _migratorByIndex;
    
    public MigratorByIndexFactory(MigrationRepository migRepo)
    {
        _migratorByIndex = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(static type => string.IsNullOrEmpty(type.Namespace) == false &&
                                  type.Namespace.StartsWith("CheckMade.DevOps.TlgInputDetailsMigration.Migrators") &&
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

    public ResultOld<MigratorBase> GetMigrator(string migIndex) => 
        _migratorByIndex.TryGetValue(migIndex, out var migrator) switch
        {
            true => migrator,
            false => Ui("No migrator called 'Mig{0}' was found.", migIndex)
        };
}
