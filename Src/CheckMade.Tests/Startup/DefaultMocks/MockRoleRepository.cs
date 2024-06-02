using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;

namespace CheckMade.Tests.Startup.DefaultMocks;

internal class MockRoleRepository : IRoleRepository
{
    public Task<IEnumerable<Role>> GetAllOrThrowAsync()
    {
        var builder = ImmutableArray.CreateBuilder<Role>();
        
        builder.AddRange(
            new Role("token1", RoleType.SanitaryOps_Admin),
            new Role("token2", RoleType.SanitaryOps_Inspector),
            new Role("token3", RoleType.SanitaryOps_Engineer),
            new Role("token4", RoleType.SanitaryOps_CleanLead),
            new Role("token5", RoleType.SanitaryOps_Observer));
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}