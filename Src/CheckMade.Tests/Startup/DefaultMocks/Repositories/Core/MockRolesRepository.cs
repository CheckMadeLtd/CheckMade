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
            SOpsAdmin_DanielEn_X2024,
            SOpsInspector_DanielEn_X2024,
            SOpsEngineer_DanielEn_X2024,
            SOpsCleanLead_DanielDe_X2024,
            SOpsObserver_DanielEn_X2024,
            
            SOpsInspector_DanielDe_X2024,
            SOpsEngineer_DanielEn_X2024,
            SOpsCleanLead_DanielEn_X2024,
            SanitaryOpsObserver2);
        
        return Task.FromResult<IEnumerable<Role>>(builder.ToImmutable());
    }
}