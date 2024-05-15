using System.Data.Common;
using CheckMade.Common.Interfaces;
using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using Newtonsoft.Json;
using Npgsql;

namespace CheckMade.Common.Persistence;

public interface IDbExecutionHelper
{
    Task ExecuteOrThrowAsync(Func<NpgsqlCommand, Task> executeDbOperation);
}

internal class DbExecutionHelper(
        IDbConnectionProvider dbProvider,
        IDbOpenRetryPolicy dbOpenRetryPolicy,
        IDbCommandRetryPolicy dbCommandRetryPolicy) 
    : IDbExecutionHelper
{
    public async Task ExecuteOrThrowAsync(Func<NpgsqlCommand, Task> executeDbOperation)
    {
        using (var db = dbProvider.CreateConnection())
        {
            try
            {
                await dbOpenRetryPolicy.ExecuteAsync(() =>
                {
                    db.Open();
                    return Task.CompletedTask;
                });
            }
            catch (DbException dbEx)
            {
                throw new DataAccessException("Database exception has occurred during attempt to open " +
                                              "a db connection.", dbEx);
            }
            catch (Exception ex)
            {
                throw new DataAccessException("An exception with unexpected type has occurred during attempt " +
                                              "to open a db connection.", ex);
            }
            
            await using (var command = new NpgsqlCommand())
            {
                command.Connection = db as NpgsqlConnection;

                try
                {
                    await dbCommandRetryPolicy.ExecuteAsync(async () => await executeDbOperation(command));
                }
                catch (JsonSerializationException jsonEx)
                {
                    throw new DataAccessException("JSON (de)serialization exception has occurred during " +
                                                  "db command execution.", jsonEx);
                }
                catch (DbException dbEx)
                {
                    throw new DataAccessException("A PostgreSQL-specific exception has occured during db " +
                                                  "command execution.", dbEx);
                }
                catch (Exception ex)
                {
                    throw new DataAccessException("An exception has occurred during db command execution.", ex);
                }
            }
        }
    }
}