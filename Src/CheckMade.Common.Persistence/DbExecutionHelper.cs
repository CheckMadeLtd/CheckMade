using System.Data;
using System.Diagnostics;
using CheckMade.Abstract.Domain.Interfaces.Persistence;
using CheckMade.Common.Utils.RetryPolicies;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CheckMade.Common.Persistence;

public interface IDbExecutionHelper
{
    Task ExecuteAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task> executeDbOperation);
}

public sealed class DbExecutionHelper(
    IDbConnectionProvider dbProvider,
    IDbOpenRetryPolicy dbOpenRetryPolicy,
    IDbCommandRetryPolicy dbCommandRetryPolicy,
    ILogger<DbExecutionHelper> logger) 
    : IDbExecutionHelper
{
    public async Task ExecuteAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task> executeDbOperations)
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
                var stopwatch = Stopwatch.StartNew();
                await executeDbOperations(db, transaction);
                stopwatch.Stop();

                const int currentWarningThreshold = 750;
                
                if (stopwatch.ElapsedMilliseconds > currentWarningThreshold)
                {
                    logger.LogWarning($"DB query took {stopwatch.ElapsedMilliseconds}ms, i.e. longer than " +
                                      $"the current warning threshold ({currentWarningThreshold}ms).");
                }
                
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