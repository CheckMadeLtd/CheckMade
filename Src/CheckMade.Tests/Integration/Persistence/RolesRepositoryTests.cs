using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public class RolesRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetAsync_ReturnsSpecificTestUser()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var repo = _services.GetRequiredService<IRolesRepository>();
        var role = await repo.GetAsync(SanitaryInspector_LukasDe_X2024);

        Assert.NotNull(role);
        Assert.True(role.Equals(SanitaryInspector_LukasDe_X2024));
    }
    
    [Fact]
    public async Task GetAllAsync_ContainsSpecificRole_FromTestSeedingData()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        var repo = _services.GetRequiredService<IRolesRepository>();
        var roles = await repo.GetAllAsync();
        
        Assert.Contains(
            SanitaryAdmin_DanielEn_X2024.Token,
            roles.Select(r => r.Token));
    }
}