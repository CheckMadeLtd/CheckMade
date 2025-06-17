using System.Data;

namespace CheckMade.Abstract.Domain.Interfaces.Persistence;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
