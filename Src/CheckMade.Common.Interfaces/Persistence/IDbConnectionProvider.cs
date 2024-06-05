using System.Data;

namespace CheckMade.Common.Interfaces.Persistence;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
