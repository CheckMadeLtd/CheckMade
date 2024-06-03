using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories;

internal class MockRoleRepository : IRoleRepository
{
    public Task<IEnumerable<Role>> GetAllOrThrowAsync()
    {
        var builder = ImmutableArray.CreateBuilder<Role>();
        
        builder.AddRange(
            TestUtils.SanitaryOpsAdmin1,
            TestUtils.SanitaryOpsInspector1,
            TestUtils.SanitaryOpsEngineer1,
            TestUtils.SanitaryOpsCleanLead1,
            TestUtils.SanitaryOpsObserver1);
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}