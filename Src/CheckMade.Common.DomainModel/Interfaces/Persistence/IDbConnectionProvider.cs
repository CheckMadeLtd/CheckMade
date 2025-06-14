using System.Data;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
