using System.Data;

namespace CheckMade.Common.Domain.Interfaces.Persistence;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
