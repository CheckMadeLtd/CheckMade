using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class TlgClientPortRoleRepositoryTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData(InteractionMode.Operations)]
    [InlineData(InteractionMode.Communications)]
    [InlineData(InteractionMode.Notifications)]
    public async Task SavesAndRetrieves_OneTlgClientPortRole_WhenInputValid(InteractionMode mode)
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var inputPortRole = new TlgClientPortRole(
            ITestUtils.IntegrationTestsRole,
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_02, mode),
            DateTime.UtcNow,
            Option<DateTime>.None());

        var repo = _services.GetRequiredService<ITlgClientPortRoleRepository>();

        await repo.AddAsync(inputPortRole);
        var retrieved = (await repo.GetAllAsync())
            .MaxBy(cpr => cpr.ActivationDate);
        await repo.HardDeleteAsync(inputPortRole);
        
        Assert.Equivalent(inputPortRole.Role, retrieved!.Role);
        Assert.Equivalent(inputPortRole.ClientPort, retrieved.ClientPort);
    }

    [Theory]
    [InlineData(InteractionMode.Operations)]
    [InlineData(InteractionMode.Communications)]
    [InlineData(InteractionMode.Notifications)]
    public async Task SuccessfullyUpdatesStatus_FromActiveToHistoric(InteractionMode mode)
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var preExistingActivePortRole = new TlgClientPortRole(
            ITestUtils.IntegrationTestsRole,
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_02, mode),
            DateTime.UtcNow,
            Option<DateTime>.None(),
            DbRecordStatus.Active);

        var repo = _services.GetRequiredService<ITlgClientPortRoleRepository>();
        
        await repo.AddAsync(preExistingActivePortRole);
        await repo.UpdateStatusAsync(preExistingActivePortRole, DbRecordStatus.Historic);
        
        var retrievedUpdated = (await repo.GetAllAsync())
            .MaxBy(cpr => cpr.ActivationDate);
        
        await repo.HardDeleteAsync(preExistingActivePortRole);
        
        Assert.Equivalent(preExistingActivePortRole.ClientPort, retrievedUpdated!.ClientPort);
        Assert.Equal(DbRecordStatus.Historic, retrievedUpdated.Status);
        Assert.True(retrievedUpdated.DeactivationDate.IsSome);
    }
}