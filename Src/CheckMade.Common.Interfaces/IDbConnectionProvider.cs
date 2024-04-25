using System.Data;

namespace CheckMade.Common.Interfaces;

public interface IDbConnectionProvider
{
    IDbConnection CreateConnection();
}
