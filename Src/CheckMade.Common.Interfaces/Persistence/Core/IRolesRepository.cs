using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IRolesRepository
{
    Task<IReadOnlyCollection<Role>> GetAllAsync();
}