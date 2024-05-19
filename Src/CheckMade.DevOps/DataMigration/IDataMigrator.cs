using CheckMade.Common.FpExt.MonadicWrappers;

namespace CheckMade.DevOps.DataMigration;

internal interface IDataMigrator
{
    Task<Attempt<int>> MigrateAsync(string env);
}
