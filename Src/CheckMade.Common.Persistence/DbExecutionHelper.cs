using System.Data.Common;
using CheckMade.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace CheckMade.Common.Persistence;

public interface IDbExecutionHelper
{
    Task ExecuteAsync(Func<NpgsqlCommand, Task> executeDbOperation);
}

internal class DbExecutionHelper(
        IDbConnectionProvider dbProvider, 
        ILogger<DbExecutionHelper> logger) 
    : IDbExecutionHelper
{
    public async Task ExecuteAsync(Func<NpgsqlCommand, Task> executeDbOperation)
    {
        using (var db = dbProvider.CreateConnection())
        {
            try
            {
                db.Open();
            }
            catch (DbException dbEx)
            {
                logger.LogError("Database exception upon attempt to open connection: " +
                                "{exMessage}", dbEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError("An unexpected exception type has been thrown while opening a db connection:" +
                                " {exMessage}", ex.Message);
                throw;
            }
            
            await using (var command = new NpgsqlCommand())
            {
                command.Connection = db as NpgsqlConnection;

                try
                {
                    await executeDbOperation(command);
                }
                catch (JsonSerializationException jsonEx)
                {
                    logger.LogError("JSON (de)serialization exception has occurred during command execution: " + 
                                    "{exMessage}", jsonEx.Message);
                    throw;
                }
                catch (NpgsqlException npgEx)
                {
                    logger.LogError("A PostgreSQL-specific exception has occured during command execution: " +
                                    "{exMessage}", npgEx.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError("An exception has occurred during command execution: " +
                                    "{exMessage}", ex.Message);
                    throw;
                }
            }
        }
    }
}