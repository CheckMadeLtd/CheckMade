using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class RolesRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetAllAsync_ContainsSpecificRole_FromTestSeedingData()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var repo = _services.GetRequiredService<IRolesRepository>();
        var roles = await repo.GetAllAsync();

        // This role should have been added via seeding/for_ci_db.sql (or similar)
        Assert.Contains(IntegrationTests_Role_Default.Token, roles.Select(r => r.Token));
    }
}