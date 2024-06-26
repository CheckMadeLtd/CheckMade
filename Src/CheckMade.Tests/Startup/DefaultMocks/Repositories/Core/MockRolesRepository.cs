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
            SanitaryOpsInspector1_HasRoleBindings_ForAllModes,
            SanitaryOpsEngineer1,
            SanitaryOpsCleanLead1_German,
            SanitaryOpsObserver1,
            
            SanitaryOpsInspector2_HasNoBindings_German,
            SanitaryOpsEngineer2_HasBindOnlyIn_CommunicationsMode,
            SanitaryOpsCleanLead2,
            SanitaryOpsObserver2);
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}