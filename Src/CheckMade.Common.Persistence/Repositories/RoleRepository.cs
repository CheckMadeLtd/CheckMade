using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;

namespace CheckMade.Common.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    public Task<IEnumerable<Role>> GetAllOrThrowAsync()
    {
        var builder = ImmutableArray.CreateBuilder<Role>();
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}