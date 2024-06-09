using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Tlg;

namespace CheckMade.Common.Persistence.Repositories.Tlg;

public class TlgClientPortToRoleMapRepository : ITlgClientPortToRoleMapRepository
{
    public Task<IEnumerable<TlgClientPortToRoleMap>> GetAllAsync()
    {
        var builder = ImmutableList.CreateBuilder<TlgClientPortToRoleMap>();
        
        return Task.FromResult<IEnumerable<TlgClientPortToRoleMap>>(builder.ToImmutable());
    }
}