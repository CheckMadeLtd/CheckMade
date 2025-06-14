using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public sealed class TlgAgentRoleBindingsRepositoryTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task SavesAndRetrieves_OneTlgAgentRoleBind_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var repo = _services.GetRequiredService<ITlgAgentRoleBindingsRepository>();
        
        var newInputTlgAgentRoleBind = 
            TestRepositoryUtils.GetNewRoleBind(
                SanitaryAdmin_DanielEn_X2024, 
                PrivateBotChat_Operations);
        
        await repo.AddAsync(newInputTlgAgentRoleBind);
        var retrieved = 
            (await repo.GetAllAsync())
            .MaxBy(static tarb => tarb.ActivationDate);
        await repo.HardDeleteAsync(newInputTlgAgentRoleBind);
        
        Assert.Equal(
            newInputTlgAgentRoleBind.Role.Token,
            retrieved!.Role.Token);
        Assert.Equivalent(
            newInputTlgAgentRoleBind.TlgAgent,
            retrieved.TlgAgent);
    }

    [Fact]
    public async Task SuccessfullyUpdatesStatus_FromActiveToHistoric()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        var activeTlgAgentRole = 
            TestRepositoryUtils.GetNewRoleBind(
                SanitaryAdmin_DanielEn_X2024,
                PrivateBotChat_Operations);

        var repo = _services.GetRequiredService<ITlgAgentRoleBindingsRepository>();
        
        await repo.AddAsync(activeTlgAgentRole);
        await repo.UpdateStatusAsync(
            activeTlgAgentRole,
            DbRecordStatus.Historic);
        var retrievedUpdated = 
            (await repo.GetAllAsync())
            .MaxBy(static tarb => tarb.ActivationDate);
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