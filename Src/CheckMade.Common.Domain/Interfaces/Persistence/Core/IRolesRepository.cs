using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Common.Domain.Interfaces.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Persistence.Core;

public interface IRolesRepository
{
    Task<Role?> GetAsync(IRoleInfo role);
    Task<IReadOnlyCollection<Role>> GetAllAsync();
}