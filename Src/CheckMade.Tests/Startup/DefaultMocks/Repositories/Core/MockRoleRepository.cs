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
            ITestUtils.SanitaryOpsAdmin1,
            ITestUtils.SanitaryOpsInspector1,
            ITestUtils.SanitaryOpsEngineer1,
            ITestUtils.SanitaryOpsCleanLead1,
            ITestUtils.SanitaryOpsObserver1,
            
            ITestUtils.SanitaryOpsInspector2,
            ITestUtils.SanitaryOpsEngineer2,
            ITestUtils.SanitaryOpsCleanLead2,
            ITestUtils.SanitaryOpsObserver2
            );
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}