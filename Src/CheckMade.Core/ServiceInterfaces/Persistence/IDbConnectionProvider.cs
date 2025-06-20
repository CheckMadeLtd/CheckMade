using System.Data;

namespace CheckMade.Core.ServiceInterfaces.Persistence;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
