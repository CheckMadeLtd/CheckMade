using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class RoleRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetAllAsync_OneRole_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var repo = _services.GetRequiredService<IRoleRepository>();
        var roles = await repo.GetAllAsync();

        // This role should have been added via seeding/for_integration_tests.sql (or similar)
        Assert.Contains(ITestUtils.IntegrationTestsRole.Token, roles.Select(r => r.Token));
    }
}