using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Core;

internal class MockRoleRepository : IRoleRepository
{
    public Task<IEnumerable<Role>> GetAllAsync()
    {
        var builder = ImmutableArray.CreateBuilder<Role>();
        
        builder.AddRange(
            TestUtils.SanitaryOpsAdmin1,
            TestUtils.SanitaryOpsInspector1,
            TestUtils.SanitaryOpsEngineer1,
            TestUtils.SanitaryOpsCleanLead1,
            TestUtils.SanitaryOpsObserver1,
            
            TestUtils.SanitaryOpsInspector2,
            TestUtils.SanitaryOpsEngineer2,
            TestUtils.SanitaryOpsCleanLead2,
            TestUtils.SanitaryOpsObserver2
            );
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}