using CheckMade.Common.Model;

namespace CheckMade.Common.Interfaces.Persistence;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
}