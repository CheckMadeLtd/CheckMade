using System.Data;
using CheckMade.Abstract.Domain.ServiceInterfaces.Persistence;
using Npgsql;

namespace CheckMade.Services.Persistence;

public sealed class DbConnectionProvider(string connectionString) : IDbConnectionProvider
{
    // In the Unix Env. (including locally and on GitHub Runner) the var names/keys need to use '_'
    // but in Azure Keyvault they need to use '-'
    
    public const string KeyToLocalDbConnStringInEnv = "PG_DB_CONNSTRING"; // 'Local' is relative, can be e.g. Dev or CI
    public const string KeyToPrdDbConnStringInKeyvault = "POSTGRESQLCONNSTR_PRD-DB";
    public const string DbPswPlaceholderString = "MYSECRET";
    public const string KeyToPrdDbPswInKeyvaultOrSecrets = "ConnectionStrings:PRD-DB-PSW";
    public const string KeyToPrdDbConnStringWithPswInEnv = "FOR_TESTS_AND_DEVOPS_PG_PRD_DB_CONNSTRING";
    
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}
