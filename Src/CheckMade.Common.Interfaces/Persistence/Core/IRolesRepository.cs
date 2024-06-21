using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IRolesRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
}