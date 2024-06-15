using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public class TlgClientPortModeRoleRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task SavesAndRetrieves_OneTlgClientPortModeRole_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var existingTestRole = new Role("AAA111", RoleType.SanitaryOps_Inspector);
        
        var inputPortModeRole = new TlgClientPortModeRole(
            existingTestRole,
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_02),
            DateTime.Now,
            Option<DateTime>.None());

        var repo = _services.GetRequiredService<ITlgClientPortModeRoleRepository>();

        await repo.AddAsync(inputPortModeRole);
        var retrieved = (await repo.GetAllAsync())
            .MaxBy(cpr => cpr.ActivationDate);
        await repo.HardDeleteAsync(inputPortModeRole);
        
        Assert.Equivalent(inputPortModeRole.Role, retrieved!.Role);
        Assert.Equivalent(inputPortModeRole.ClientPort, retrieved.ClientPort);
    }

    [Fact]
    public async Task SuccessfullyUpdatesStatus_FromActiveToHistoric()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var existingTestRole = new Role("AAA111", RoleType.SanitaryOps_Inspector);
        
        var preExistingActivePortModeRole = new TlgClientPortModeRole(
            existingTestRole,
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_02),
            DateTime.Now,
            Option<DateTime>.None(),
            DbRecordStatus.Active);

        var repo = _services.GetRequiredService<ITlgClientPortModeRoleRepository>();
        await repo.AddAsync(preExistingActivePortModeRole);
        
        await repo.UpdateStatusAsync(preExistingActivePortModeRole, DbRecordStatus.Historic);
        
        var retrievedUpdated = (await repo.GetAllAsync())
            .MaxBy(cpr => cpr.ActivationDate);
        
        await repo.HardDeleteAsync(preExistingActivePortModeRole);
        
        Assert.Equal(DbRecordStatus.Historic, retrievedUpdated!.Status);
        Assert.True(retrievedUpdated.DeactivationDate.IsSome);
    }
}