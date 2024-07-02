using CheckMade.Common.Model.Core.Actors;

namespace CheckMade.Common.Interfaces.Persistence.Core;

public interface IRolesRepository
{
    Task<IReadOnlyCollection<Role>> GetAllAsync();
}