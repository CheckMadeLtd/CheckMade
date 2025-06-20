using System.Data;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
