using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Persistence;
using CheckMade.Common.Utils.Generic;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CheckMade.Tests.Integration;

public class TlgInputRepositoryTests(ITestOutputHelper testOutputHelper)
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task SavesAndRetrievesOneInput_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        
        var tlgInputs = new[]
        {
            utils.GetValidTlgTextMessage(),
            utils.GetValidTlgTextMessage(),
            utils.GetValidTlgTextMessage()
        };
        
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();

        foreach (var input in tlgInputs)
        {
            var expectedRetrieval = new List<TlgInput>
            {
                new (input.UserId, input.ChatId, input.InteractionMode, input.TlgInputType, input.Details)
            };
        
            await inputRepo.AddAsync(input);
            var retrievedInputs = 
                (await inputRepo.GetAllAsync(input.UserId))
                .OrderByDescending(x => x.Details.TlgDate)
                .ToList().AsReadOnly();
            await inputRepo.HardDeleteAllAsync(input.UserId);
        
            Assert.Equivalent(expectedRetrieval[0], retrievedInputs[0]);
        }
    }

    [Fact]
    public async Task AddAsync_And_GetAllAsync_CorrectlyAddAndReturn_MultipleValidInputs()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        TlgUserId userId = utils.Randomizer.GenerateRandomLong();
        
        var tlgInputs = new[]
        {
            utils.GetValidTlgTextMessage(userId),
            utils.GetValidTlgTextMessage(userId),
            utils.GetValidTlgTextMessage(userId)
        };
        
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();
        
        await inputRepo.AddAsync(tlgInputs);
        var retrievedInputs = await inputRepo.GetAllAsync(userId);
        await inputRepo.HardDeleteAllAsync(userId);

        Assert.Equivalent(tlgInputs, retrievedInputs);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIdNotExist()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var randomizer = _services.GetRequiredService<Randomizer>();
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();
        TlgUserId userId = randomizer.GenerateRandomLong();
    
        var retrievedInputs = await inputRepo.GetAllAsync(userId);
    
        Assert.Empty(retrievedInputs);
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
        
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();
        
        // No assert needed: test fails when exception thrown!
        await inputRepo.GetAllAsync(devDbUserId);
    }
}