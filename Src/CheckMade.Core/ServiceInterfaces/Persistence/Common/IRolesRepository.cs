using CheckMade.Core.Model.Common.Actors;

namespace CheckMade.Core.ServiceInterfaces.Persistence.Common;

public interface IRolesRepository
{
    Task<Role?> GetAsync(IRoleInfo role);
    Task<IReadOnlyCollection<Role>> GetAllAsync();
}