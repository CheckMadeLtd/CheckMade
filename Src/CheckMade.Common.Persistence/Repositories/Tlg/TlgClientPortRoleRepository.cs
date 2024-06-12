using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Common.Persistence.Repositories.Tlg;

public class TlgClientPortRoleRepository : ITlgClientPortRoleRepository
{
    public Task<IEnumerable<TlgClientPortRole>> GetAllAsync()
    {
        var builder = ImmutableList.CreateBuilder<TlgClientPortRole>();
        
        return Task.FromResult<IEnumerable<TlgClientPortRole>>(builder.ToImmutable());
    }
}