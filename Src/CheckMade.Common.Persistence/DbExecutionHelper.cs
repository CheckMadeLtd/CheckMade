using CheckMade.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CheckMade.Common.Persistence;

public interface IDbExecutionHelper
{
    Task ExecuteAsync(Func<NpgsqlCommand, Task> executeDbOperation);
}

internal class DbExecutionHelper(IDbConnectionProvider dbProvider, ILogger<DbExecutionHelper> logger) 
    : IDbExecutionHelper
{
    public async Task ExecuteAsync(Func<NpgsqlCommand, Task> executeDbOperation)
    {
        using (var db = dbProvider.CreateConnection())
        {
            db.Open();
            
            await using (var command = new NpgsqlCommand())
            {
                command.Connection = db as NpgsqlConnection;
                
                try
                {
                    await executeDbOperation(command);
                }
                catch (Exception ex)
                {
                    logger.LogError("Database exception thrown: {exMessage}", ex.Message);
                    throw;
                }
            }
        }
    }
}