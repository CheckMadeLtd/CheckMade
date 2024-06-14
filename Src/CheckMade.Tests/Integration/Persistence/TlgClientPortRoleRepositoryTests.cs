using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public class TlgClientPortRoleRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task SavesAndRetrieves_OneTlgClientPortRole_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var existingTestRole = new Role("AAA111", RoleType.SanitaryOps_Inspector);
        
        var inputPortRole = new TlgClientPortRole(
            existingTestRole,
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_02),
            DateTime.Now,
            Option<DateTime>.None());

        var repo = _services.GetRequiredService<ITlgClientPortRoleRepository>();

        await repo.AddAsync(inputPortRole);
        var retrieved = (await repo.GetAllAsync()).MaxBy(cpr => cpr.ActivationDate);
        await repo.HardDeleteAsync(inputPortRole);
        
        Assert.Equivalent(inputPortRole, retrieved);
    }
}