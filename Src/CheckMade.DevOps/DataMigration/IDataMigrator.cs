using CheckMade.Common.Utils;
using CheckMade.Common.Utils.MonadicWrappers;

namespace CheckMade.DevOps.DataMigration;

internal interface IDataMigrator
{
    Task<Result<int>> MigrateAsync(string env);
}
