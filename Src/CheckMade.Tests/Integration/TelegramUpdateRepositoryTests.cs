using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Persistence;
using CheckMade.Common.Utils.Generic;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CheckMade.Tests.Integration;

public class TelegramUpdateRepositoryTests(ITestOutputHelper testOutputHelper)
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task SavesAndRetrievesOneUpdate_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var telegramUpdates = new[]
        {
            utils.GetValidModelInputTextMessage(),
            utils.GetValidModelInputTextMessage(),
            utils.GetValidModelInputTextMessage()
        };
        var updateRepo = _services.GetRequiredService<ITelegramUpdateRepository>();

        foreach (var update in telegramUpdates)
        {
            var expectedRetrieval = new List<TelegramUpdate>
            {
                new (update.UserId, update.TelegramChatId, update.BotType, update.ModelUpdateType, update.Details)
            };
        
            await updateRepo.AddAsync(update);
            var retrievedUpdates = 
                (await updateRepo.GetAllAsync(update.UserId))
                .OrderByDescending(x => x.Details.TelegramDate)
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
        TelegramUserId userId = utils.Randomizer.GenerateRandomLong();
        
        var telegramUpdates = new[]
        {
            utils.GetValidModelInputTextMessage(userId),
            utils.GetValidModelInputTextMessage(userId),
            utils.GetValidModelInputTextMessage(userId)
        };
        var updateRepo = _services.GetRequiredService<ITelegramUpdateRepository>();
        
        await updateRepo.AddAsync(telegramUpdates);
        var retrievedUpdates = await updateRepo.GetAllAsync(userId);
        await updateRepo.HardDeleteAllAsync(userId);

        Assert.Equivalent(telegramUpdates, retrievedUpdates);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIdNotExist()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var randomizer = _services.GetRequiredService<Randomizer>();
        var updateRepo = _services.GetRequiredService<ITelegramUpdateRepository>();
        TelegramUserId userId = randomizer.GenerateRandomLong();
    
        var retrievedUpdates = await updateRepo.GetAllAsync(userId);
    
        Assert.Empty(retrievedUpdates);
    }

    /* Main purpose is to verify that the Details column doesn't have values with outdated schema e.g. because
    its migration has been forgotten after the details schema evolved in the model/code. */ 
    // [Theory(Skip = "Waiting to migrate the old DB data")]
    // [Theory(Skip = "Running tests from unknown IP / internet")]
    [Theory]
    [InlineData(TestUtils.TestUserDanielGorinTelegramId, false)]
    [InlineData(TestUtils.TestUserDanielGorinTelegramId, true)]
    public async Task Verifies_Db_DoesNotHaveInvalidTestData_ForGivenTestUser(
        TelegramUserId devDbUserId, bool overwriteDefaultDbConnProviderWithPrdDbConn)
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
        
        var updateRepo = _services.GetRequiredService<ITelegramUpdateRepository>();
        
        // No assert needed: test fails when exception thrown!
        await updateRepo.GetAllAsync(devDbUserId);
    }
}