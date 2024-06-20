using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class TlgAgentRoleBindingsRepositoryTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData(InteractionMode.Operations)]
    [InlineData(InteractionMode.Communications)]
    [InlineData(InteractionMode.Notifications)]
    public async Task SavesAndRetrieves_OneTlgAgentRole_WhenInputValid(InteractionMode mode)
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var inputTlgAgentRole = new TlgAgentRoleBind(
            ITestUtils.IntegrationTestsRole,
            new TlgAgent(ITestUtils.TestUserId_03, ITestUtils.TestChatId_02, mode),
            DateTime.UtcNow,
            Option<DateTime>.None());

        var repo = _services.GetRequiredService<ITlgAgentRoleBindingsRepository>();

        await repo.AddAsync(inputTlgAgentRole);
        var retrieved = (await repo.GetAllAsync())
            .MaxBy(arb => arb.ActivationDate);
        await repo.HardDeleteAsync(inputTlgAgentRole);
        
        Assert.Equivalent(inputTlgAgentRole.Role, retrieved!.Role);
        Assert.Equivalent(inputTlgAgentRole.TlgAgent, retrieved.TlgAgent);
    }

    [Theory]
    [InlineData(InteractionMode.Operations)]
    [InlineData(InteractionMode.Communications)]
    [InlineData(InteractionMode.Notifications)]
    public async Task SuccessfullyUpdatesStatus_FromActiveToHistoric(InteractionMode mode)
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var preExistingActiveTlgAgentRole = new TlgAgentRoleBind(
            ITestUtils.IntegrationTestsRole,
            new TlgAgent(ITestUtils.TestUserId_03, ITestUtils.TestChatId_02, mode),
            DateTime.UtcNow,
            Option<DateTime>.None(),
            DbRecordStatus.Active);

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