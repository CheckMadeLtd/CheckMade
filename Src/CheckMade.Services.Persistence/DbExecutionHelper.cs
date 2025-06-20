using System.Data;
using System.Diagnostics;
using CheckMade.Core.ServiceInterfaces.Persistence;
using General.Utils.RetryPolicies;
using Microsoft.Extensions.Logging;
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
    IDbCommandRetryPolicy dbCommandRetryPolicy,
    ILogger<DbExecutionHelper> logger) 
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
                var stopwatch = Stopwatch.StartNew();
                await executeDbOperations(db, transaction);
                stopwatch.Stop();

                const int currentWarningThreshold = 500;
                
                if (stopwatch.ElapsedMilliseconds > currentWarningThreshold)
                {
                    logger.LogWarning($"DB query took {stopwatch.ElapsedMilliseconds}ms, i.e. longer than " +
                                      $"the current warning threshold ({currentWarningThreshold}ms).");
                }
                
                // For detailed db perf logging.
                
                // // replacements needed for Application Insights, otherwise we get one log entry per line!
                // var sqlCommands = string.Join("; ", 
                //     commands.Select(static cmd => 
                //         cmd.CommandText.Replace('\n', ' ').Replace('\r', ' ')));
                //
                // Console.WriteLine("Performance debugging - query took {0}ms - SQL commands: {1}", 
                //     stopwatch.ElapsedMilliseconds, sqlCommands);
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