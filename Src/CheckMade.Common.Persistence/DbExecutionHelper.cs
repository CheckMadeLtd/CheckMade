using System.Data.Common;
using CheckMade.Common.Interfaces;
using CheckMade.Common.Utils;
using Newtonsoft.Json;
using Npgsql;

namespace CheckMade.Common.Persistence;

public interface IDbExecutionHelper
{
    Task ExecuteAsync(Func<NpgsqlCommand, Task> executeDbOperation);
}

internal class DbExecutionHelper(IDbConnectionProvider dbProvider) : IDbExecutionHelper
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
                    await executeDbOperation(command);
                }
                catch (JsonSerializationException jsonEx)
                {
                    throw new DataAccessException("JSON (de)serialization exception has occurred during " +
                                                  "db command execution", jsonEx);
                }
                catch (NpgsqlException npgEx)
                {
                    throw new DataAccessException("A PostgreSQL-specific exception has occured during db " +
                                                  "command execution", npgEx);
                }
                catch (Exception ex)
                {
                    throw new DataAccessException("An exception has occurred during db command execution", ex);
                }
            }
        }
    }
}