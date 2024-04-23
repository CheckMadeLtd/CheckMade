using System.Data;
using CheckMade.Common.Interfaces;
using Npgsql;

namespace CheckMade.Common.Persistence;

internal record DbConnectionProvider : IDbConnectionProvider
{
    private readonly string _connectionString;

    public DbConnectionProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
