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
            DanielIsSanitaryOpsAdminAtMockParooka2024,
            SanitaryOpsInspector1,
            SanitaryOpsEngineer1,
            SanitaryOpsCleanLead1German,
            SanitaryOpsObserver1,
            
            DanielIsSanitaryOpsInspectorAtMockHurricane2024,
            SanitaryOpsEngineer2,
            SanitaryOpsCleanLead2English,
            SanitaryOpsObserver2);
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}