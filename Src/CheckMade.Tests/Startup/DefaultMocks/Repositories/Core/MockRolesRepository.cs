using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Core;

internal class MockRolesRepository : IRolesRepository
{
    public Task<IEnumerable<Role>> GetAllAsync()
    {
        var builder = ImmutableArray.CreateBuilder<Role>();
        
        builder.AddRange(
            SanitaryOpsAdmin_AtMockParooka2024_Default,
            SanitaryOpsInspector1,
            SanitaryOpsEngineer1,
            SanitaryOpsCleanLead1_German,
            SanitaryOpsObserver1,
            
            SanitaryOpsInspector_AtMockHurricane2024_German,
            SanitaryOpsEngineer2,
            SanitaryOpsCleanLead2,
            SanitaryOpsObserver2);
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}