using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
}