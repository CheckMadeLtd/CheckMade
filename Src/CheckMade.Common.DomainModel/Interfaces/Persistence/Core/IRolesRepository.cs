using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Persistence.Core;

public interface IRolesRepository
{
    Task<Role?> GetAsync(IRoleInfo role);
    Task<IReadOnlyCollection<Role>> GetAllAsync();
}