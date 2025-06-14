using System.Data;

namespace CheckMade.Common.DomainModel.Persistence;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
