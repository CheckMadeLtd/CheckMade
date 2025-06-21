using System.Data;
using CheckMade.Core.ServiceInterfaces.Persistence;
using General.Utils.RetryPolicies;
using Npgsql;

namespace CheckMade.Services.Persistence;

public interface IDbExecutionHelper
{
    /// <summary>
    /// The collection of commands are passed purely for logging/debugging purposes, otherwise not needed 
    /// </summary>
    Task ExecuteAsync(
        Func<NpgsqlConnection, NpgsqlTransaction, Task> executeDbOperation,
        IReadOnlyCollection<NpgsqlCommand> commands);
}

public sealed class DbExecutionHelper(
    IDbConnectionProvider dbProvider,
    IDbOpenRetryPolicy dbOpenRetryPolicy,
    IDbCommandRetryPolicy dbCommandRetryPolicy) 
    : IDbExecutionHelper
{
    public async Task ExecuteAsync(
        Func<NpgsqlConnection, NpgsqlTransaction, Task> executeDbOperations,
        IReadOnlyCollection<NpgsqlCommand> commands)
    {
        await using var db = dbProvider.CreateConnection() as NpgsqlConnection;

        if (db is null)
            throw new DataException("Failed to assign IDbConnection");
        
        await dbOpenRetryPolicy.ExecuteAsync(async () => await db.OpenAsync());
        await using var transaction = await db.BeginTransactionAsync();
        
        try
        {
            await dbCommandRetryPolicy.ExecuteAsync(async () =>
            {
                await executeDbOperations(db, transaction);
            });
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}