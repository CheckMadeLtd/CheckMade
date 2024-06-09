using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Persistence.Repositories.Core;

public class RoleRepository : IRoleRepository
{
    public Task<IEnumerable<Role>> GetAllAsync()
    {
        var builder = ImmutableArray.CreateBuilder<Role>();
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}