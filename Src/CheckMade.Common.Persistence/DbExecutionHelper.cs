using System.Data;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Utils.RetryPolicies;
using Npgsql;

namespace CheckMade.Common.Persistence;

public interface IDbExecutionHelper
{
    Task ExecuteAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task> executeDbOperation);
}

internal class DbExecutionHelper(
        IDbConnectionProvider dbProvider,
        IDbOpenRetryPolicy dbOpenRetryPolicy,
        IDbCommandRetryPolicy dbCommandRetryPolicy) 
    : IDbExecutionHelper
{
    public async Task ExecuteAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task> executeDbOperations)
    {
        await using var db = dbProvider.CreateConnection() as NpgsqlConnection;

        if (db == null)
            throw new DataException("Failed to assign IDbConnection");
        
        await dbOpenRetryPolicy.ExecuteAsync(async () => await db.OpenAsync());
        await using var transaction = await db.BeginTransactionAsync();
        
        try
        {
            await dbCommandRetryPolicy.ExecuteAsync(async () => await executeDbOperations(db, transaction));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}