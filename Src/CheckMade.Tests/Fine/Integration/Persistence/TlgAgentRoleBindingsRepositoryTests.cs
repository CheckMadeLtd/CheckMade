using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class TlgAgentRoleBindingsRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task SavesAndRetrieves_OneTlgAgentRoleBind_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var repo = _services.GetRequiredService<ITlgAgentRoleBindingsRepository>();
        
        var newInputTlgAgentRoleBind = 
            MockRepositoryUtils.GetNewRoleBind(
                SOpsAdmin_DanielEn_X2024, 
                PrivateBotChat_Operations);
        
        await repo.AddAsync(newInputTlgAgentRoleBind);
        var retrieved = 
            (await repo.GetAllAsync())
            .MaxBy(tarb => tarb.ActivationDate);
        await repo.HardDeleteAsync(newInputTlgAgentRoleBind);
        
        Assert.Equivalent(
            newInputTlgAgentRoleBind.Role,
            retrieved!.Role);
        Assert.Equivalent(
            newInputTlgAgentRoleBind.TlgAgent,
            retrieved.TlgAgent);
    }

    [Fact]
    public async Task SuccessfullyUpdatesStatus_FromActiveToHistoric()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        var activeTlgAgentRole = 
            MockRepositoryUtils.GetNewRoleBind(
                IntegrationTests_SOpsInspector_DanielEn_X2024,
                PrivateBotChat_Operations);

        var repo = _services.GetRequiredService<ITlgAgentRoleBindingsRepository>();
        
        await repo.AddAsync(activeTlgAgentRole);
        await repo.UpdateStatusAsync(
            activeTlgAgentRole,
            DbRecordStatus.Historic);
        var retrievedUpdated = 
            (await repo.GetAllAsync())
            .MaxBy(tarb => tarb.ActivationDate);
        await repo.HardDeleteAsync(activeTlgAgentRole);
        
        Assert.Equivalent(
            activeTlgAgentRole.TlgAgent,
            retrievedUpdated!.TlgAgent);
        Assert.Equal(
            DbRecordStatus.Historic,
            retrievedUpdated.Status);
        Assert.True(
            retrievedUpdated.DeactivationDate.IsSome);
    }
}