using CheckMade.Common.Utils;

namespace CheckMade.DevOps.DataMigration.Migrators;

internal class Mig0001 : IDataMigrator
{
    public async Task<Result<bool>> MigrateAsync(string env)
    {
        return await Task.FromResult(new Result<bool>(true));
    }
}