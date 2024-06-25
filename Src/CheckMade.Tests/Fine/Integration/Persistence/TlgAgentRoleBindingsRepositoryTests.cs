using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class TlgAgentRoleBindingsRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task SavesAndRetrieves_OneTlgAgentRoleBind_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var inputTlgAgentRoleBind = RoleBindFor_IntegrationTests_Role_Default;
        var repo = _services.GetRequiredService<ITlgAgentRoleBindingsRepository>();

        await repo.AddAsync(inputTlgAgentRoleBind);
        var retrieved = (await repo.GetAllAsync())
            .MaxBy(arb => arb.ActivationDate);
        await repo.HardDeleteAsync(inputTlgAgentRoleBind);
        
        Assert.Equivalent(inputTlgAgentRoleBind.Role, retrieved!.Role);
        Assert.Equivalent(inputTlgAgentRoleBind.TlgAgent, retrieved.TlgAgent);
    }

    [Fact]
    public async Task SuccessfullyUpdatesStatus_FromActiveToHistoric()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var preExistingActiveTlgAgentRole = RoleBindFor_IntegrationTests_Role_Default;
        var repo = _services.GetRequiredService<ITlgAgentRoleBindingsRepository>();
        
        await repo.AddAsync(preExistingActiveTlgAgentRole);
        await repo.UpdateStatusAsync(preExistingActiveTlgAgentRole, DbRecordStatus.Historic);
        var retrievedUpdated = (await repo.GetAllAsync())
            .MaxBy(arb => arb.ActivationDate);
        await repo.HardDeleteAsync(preExistingActiveTlgAgentRole);
        
        Assert.Equivalent(preExistingActiveTlgAgentRole.TlgAgent, retrievedUpdated!.TlgAgent);
        Assert.Equal(DbRecordStatus.Historic, retrievedUpdated.Status);
        Assert.True(retrievedUpdated.DeactivationDate.IsSome);
    }
}