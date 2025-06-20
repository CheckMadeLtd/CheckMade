using CheckMade.Abstract.Domain.Model.Common.Actors;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Persistence.Common;

public interface IRolesRepository
{
    Task<Role?> GetAsync(IRoleInfo role);
    Task<IReadOnlyCollection<Role>> GetAllAsync();
}