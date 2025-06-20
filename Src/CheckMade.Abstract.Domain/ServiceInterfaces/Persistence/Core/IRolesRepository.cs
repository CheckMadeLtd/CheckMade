using CheckMade.Abstract.Domain.Model.Core.Actors;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Core;

public interface IRolesRepository
{
    Task<Role?> GetAsync(IRoleInfo role);
    Task<IReadOnlyCollection<Role>> GetAllAsync();
}