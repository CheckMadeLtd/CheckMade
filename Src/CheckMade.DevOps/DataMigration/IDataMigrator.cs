using CheckMade.Common.FpExt.MonadicWrappers;

namespace CheckMade.DevOps.DataMigration;

internal interface IDataMigrator
{
    Task<Result<int>> MigrateAsync(string env);
}
