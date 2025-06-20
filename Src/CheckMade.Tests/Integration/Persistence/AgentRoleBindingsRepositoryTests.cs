using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public sealed class AgentRoleBindingsRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task SavesAndRetrieves_OneAgentRoleBind_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var repo = _services.GetRequiredService<IAgentRoleBindingsRepository>();
        
        var newInputAgentRoleBind = 
            TestRepositoryUtils.GetNewRoleBind(
                SanitaryAdmin_DanielEn_X2024, 
                PrivateBotChat_Operations);
        
        await repo.AddAsync(newInputAgentRoleBind);
        var retrieved = 
            (await repo.GetAllAsync())
            .MaxBy(static arb => arb.ActivationDate);
        await repo.HardDeleteAsync(newInputAgentRoleBind);
        
        Assert.Equal(
            newInputAgentRoleBind.Role.Token,
            retrieved!.Role.Token);
        Assert.Equivalent(
            newInputAgentRoleBind.Agent,
            retrieved.Agent);
    }

    [Fact]
    public async Task SuccessfullyUpdatesStatus_FromActiveToHistoric()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        var activeAgentRole = 
            TestRepositoryUtils.GetNewRoleBind(
                SanitaryAdmin_DanielEn_X2024,
                PrivateBotChat_Operations);

        var repo = _services.GetRequiredService<IAgentRoleBindingsRepository>();
        
        await repo.AddAsync(activeAgentRole);
        await repo.UpdateStatusAsync(
            activeAgentRole,
            DbRecordStatus.Historic);
        var retrievedUpdated = 
            (await repo.GetAllAsync())
            .MaxBy(static arb => arb.ActivationDate);
        await repo.HardDeleteAsync(activeAgentRole);
        
        Assert.Equivalent(
            activeAgentRole.Agent,
            retrievedUpdated!.Agent);
        Assert.Equal(
            DbRecordStatus.Historic,
            retrievedUpdated.Status);
        Assert.True(
            retrievedUpdated.DeactivationDate.IsSome);
    }
}