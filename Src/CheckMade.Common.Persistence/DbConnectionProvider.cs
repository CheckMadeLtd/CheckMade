using System.Data;
using CheckMade.Common.Interfaces;
using Npgsql;

namespace CheckMade.Common.Persistence;

public class DbConnectionProvider(string connectionString) : IDbConnectionProvider
{
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}
