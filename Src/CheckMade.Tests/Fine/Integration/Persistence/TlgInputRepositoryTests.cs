using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Persistence;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class TlgInputRepositoryTests(ITestOutputHelper testOutputHelper)
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task SavesAndRetrieves_IndividualInputs_WhenAllInputsValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        
        var tlgInputs = new[]
        {
            utils.GetValidTlgInputTextMessage(),
            utils.GetValidTlgInputTextMessage(),
            utils.GetValidTlgInputTextMessage()
        };
        
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();

        foreach (var input in tlgInputs)
        {
            var expectedRetrieval = new List<TlgInput>
            {
                new (input.TlgAgent, input.InputType, input.Details)
            };
        
            await inputRepo.AddAsync(input);
            var retrievedInputs = 
                (await inputRepo.GetAllAsync(input.TlgAgent))
                .OrderByDescending(x => x.Details.TlgDate)
                .ToList().AsReadOnly();
            await inputRepo.HardDeleteAllAsync(input.TlgAgent);
        
            Assert.Equivalent(expectedRetrieval[0], retrievedInputs[0]);
        }
    }

    [Fact]
    public async Task SavesAndRetrieves_DomainTerm_ViaCustomJsonSerialization_WhenInputHasValidDomainTerm()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(
            utils.Randomizer.GenerateRandomLong(),
            utils.Randomizer.GenerateRandomLong(),
            InteractionMode.Operations);
        
        var expectedDomainTerm = Dt(LanguageCode.de);
        
        var tlgInput = utils.GetValidTlgInputCallbackQueryForDomainTerm(
            expectedDomainTerm, tlgAgent.UserId, tlgAgent.ChatId);
        
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();
        
        await inputRepo.AddAsync(tlgInput);
        var retrievedInput = 
            (await inputRepo.GetAllAsync(tlgAgent))
            .First();
        await inputRepo.HardDeleteAllAsync(tlgAgent);
        
        Assert.Equivalent(expectedDomainTerm, retrievedInput.Details.DomainTerm.GetValueOrThrow());
    }
    
    [Fact]
    public async Task SavesAndRetrieves_GeoLocation_ViaCustomJsonSerialization_WhenInputHasValidGeo()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(
            utils.Randomizer.GenerateRandomLong(),
            utils.Randomizer.GenerateRandomLong(),
            InteractionMode.Operations);
        
        const double expectedLatitudeRaw = 17.456;
        const double expectedLongitudeRaw = -23.00987;
        const float expectedUncertainty = 15.7f;
        
        var tlgInput = utils.GetValidTlgInputLocationMessage(
            expectedLatitudeRaw, expectedLongitudeRaw, expectedUncertainty, 
            tlgAgent.UserId, tlgAgent.ChatId);
        
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();
        
        await inputRepo.AddAsync(tlgInput);
        var retrievedInput = 
            (await inputRepo.GetAllAsync(tlgAgent))
            .First();
        await inputRepo.HardDeleteAllAsync(tlgAgent);
        
        Assert.Equivalent(new Geo(expectedLatitudeRaw, expectedLongitudeRaw, expectedUncertainty), 
            retrievedInput.Details.GeoCoordinates.GetValueOrThrow());
    }

    [Fact]
    public async Task AddAsync_And_GetAllAsync_CorrectlyAddAndReturnsInBulk_MultipleValidInputs()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(
            utils.Randomizer.GenerateRandomLong(),
            utils.Randomizer.GenerateRandomLong(),
            InteractionMode.Operations);
        
        var tlgInputs = new[]
        {
            utils.GetValidTlgInputTextMessage(tlgAgent.UserId, tlgAgent.ChatId),
            utils.GetValidTlgInputTextMessage(tlgAgent.UserId, tlgAgent.ChatId),
            utils.GetValidTlgInputTextMessage(tlgAgent.UserId, tlgAgent.ChatId)
        };
        
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();
        
        await inputRepo.AddAsync(tlgInputs);
        var retrievedInputs = await inputRepo.GetAllAsync(tlgAgent);
        await inputRepo.HardDeleteAllAsync(tlgAgent);

        Assert.Equivalent(tlgInputs, retrievedInputs);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIdNotExist()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var inputRepo = _services.GetRequiredService<ITlgInputRepository>();
        var tlgAgent = new TlgAgent(
            utils.Randomizer.GenerateRandomLong(),
            utils.Randomizer.GenerateRandomLong(),
            InteractionMode.Operations);
    
        var retrievedInputs = await inputRepo.GetAllAsync(tlgAgent);
    
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