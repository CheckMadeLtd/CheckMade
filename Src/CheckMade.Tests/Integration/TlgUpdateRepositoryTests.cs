using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Common.Persistence;
using CheckMade.Common.Utils.Generic;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CheckMade.Tests.Integration;

public class TlgUpdateRepositoryTests(ITestOutputHelper testOutputHelper)
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task SavesAndRetrievesOneUpdate_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        
        var tlgUpdates = new[]
        {
            utils.GetValidTlgTextMessage(),
            utils.GetValidTlgTextMessage(),
            utils.GetValidTlgTextMessage()
        };
        
        var updateRepo = _services.GetRequiredService<ITlgUpdateRepository>();

        foreach (var update in tlgUpdates)
        {
            var expectedRetrieval = new List<TlgUpdate>
            {
                new (update.UserId, update.ChatId, update.BotType, update.TlgUpdateType, update.Details)
            };
        
            await updateRepo.AddAsync(update);
            var retrievedUpdates = 
                (await updateRepo.GetAllAsync(update.UserId))
                .OrderByDescending(x => x.Details.TlgDate)
                .ToList().AsReadOnly();
            await updateRepo.HardDeleteAllAsync(update.UserId);
        
            Assert.Equivalent(expectedRetrieval[0], retrievedUpdates[0]);
        }
    }

    [Fact]
    public async Task AddAsync_And_GetAllAsync_CorrectlyAddAndReturn_MultipleValidUpdates()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        TlgUserId userId = utils.Randomizer.GenerateRandomLong();
        
        var tlgUpdates = new[]
        {
            utils.GetValidTlgTextMessage(userId),
            utils.GetValidTlgTextMessage(userId),
            utils.GetValidTlgTextMessage(userId)
        };
        
        var updateRepo = _services.GetRequiredService<ITlgUpdateRepository>();
        
        await updateRepo.AddAsync(tlgUpdates);
        var retrievedUpdates = await updateRepo.GetAllAsync(userId);
        await updateRepo.HardDeleteAllAsync(userId);

        Assert.Equivalent(tlgUpdates, retrievedUpdates);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIdNotExist()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var randomizer = _services.GetRequiredService<Randomizer>();
        var updateRepo = _services.GetRequiredService<ITlgUpdateRepository>();
        TlgUserId userId = randomizer.GenerateRandomLong();
    
        var retrievedUpdates = await updateRepo.GetAllAsync(userId);
    
        Assert.Empty(retrievedUpdates);
    }

    /* Main purpose is to verify that the Details column doesn't have values with outdated schema e.g. because
    its migration has been forgotten after the details schema evolved in the model/code. */ 
    // [Theory(Skip = "Waiting to migrate the old DB data")]
    // [Theory(Skip = "Running tests from unknown IP / internet")]
    [Theory]
    [InlineData(ITestUtils.TestUserDanielGorinTelegramId, false)]
    [InlineData(ITestUtils.TestUserDanielGorinTelegramId, true)]
    public async Task Verifies_Db_DoesNotHaveInvalidTestData_ForGivenTestUser(
        TlgUserId devDbUserId, bool overwriteDefaultDbConnProviderWithPrdDbConn)
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        if (overwriteDefaultDbConnProviderWithPrdDbConn)
        {
            var prdDbConnString = _services.GetRequiredService<PrdDbConnStringProvider>().Get;
            testOutputHelper.WriteLine(prdDbConnString);
            var serviceCollection = new IntegrationTestStartup().Services;
            serviceCollection.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(prdDbConnString));
            _services = serviceCollection.BuildServiceProvider();
        }
        
        var updateRepo = _services.GetRequiredService<ITlgUpdateRepository>();
        
        // No assert needed: test fails when exception thrown!
        await updateRepo.GetAllAsync(devDbUserId);
    }
}