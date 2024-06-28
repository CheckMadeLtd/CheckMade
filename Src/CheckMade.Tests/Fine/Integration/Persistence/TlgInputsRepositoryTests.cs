using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Persistence;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CheckMade.Tests.Fine.Integration.Persistence;

public class TlgInputsRepositoryTests(ITestOutputHelper testOutputHelper)
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task SavesAndRetrieves_IndividualInputs_WhenAllInputsValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var inputRepo = _services.GetRequiredService<ITlgInputsRepository>();
        
        var tlgInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(roleSetting: TestOriginatorRoleSetting.IntegrationTestDefault),
            inputGenerator.GetValidTlgInputTextMessage(roleSetting: TestOriginatorRoleSetting.IntegrationTestDefault),
            inputGenerator.GetValidTlgInputTextMessage(roleSetting: TestOriginatorRoleSetting.IntegrationTestDefault)
        };
        
        foreach (var input in tlgInputs)
        {
            var expectedRetrieval = new List<TlgInput>
            {
                new (input.TlgAgent, 
                    input.InputType, 
                    input.OriginatorRole, 
                    input.LiveEventContext, 
                    input.Details)
            };
        
            await inputRepo.AddAsync(input);
            
            var retrievedInputs = 
                (await inputRepo.GetAllAsync(input.TlgAgent))
                .OrderByDescending(x => x.Details.TlgDate)
                .ToImmutableReadOnlyCollection();
            
            await inputRepo.HardDeleteAllAsync(input.TlgAgent);
        
            Assert.Equivalent(expectedRetrieval[0], retrievedInputs.First());
        }
    }

    [Fact]
    public async Task SavesAndRetrieves_DomainTerm_ViaCustomJsonSerialization_WhenInputHasValidDomainTerm()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        
        var expectedDomainTerm = Dt(LanguageCode.de);
        var tlgInput = inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(expectedDomainTerm);
        var inputRepo = _services.GetRequiredService<ITlgInputsRepository>();
        
        await inputRepo.AddAsync(tlgInput);
        var retrievedInput = 
            (await inputRepo.GetAllAsync(TlgAgent_PrivateChat_Default))
            .First();
        await inputRepo.HardDeleteAllAsync(TlgAgent_PrivateChat_Default);
        
        Assert.Equivalent(expectedDomainTerm, retrievedInput.Details.DomainTerm.GetValueOrThrow());
    }
    
    [Fact]
    public async Task SavesAndRetrieves_GeoLocation_ViaCustomJsonSerialization_WhenInputHasValidGeo()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var inputRepo = _services.GetRequiredService<ITlgInputsRepository>();
        
        var expectedGeo = new Geo(17.456, -23.00987, 15.7f);
        
        var tlgInput = inputGenerator.GetValidTlgInputLocationMessage(
            expectedGeo.Latitude, 
            expectedGeo.Longitude,
            expectedGeo.UncertaintyRadiusInMeters);
        
        await inputRepo.AddAsync(tlgInput);
        var retrievedInput = 
            (await inputRepo.GetAllAsync(TlgAgent_PrivateChat_Default))
            .First();
        await inputRepo.HardDeleteAllAsync(TlgAgent_PrivateChat_Default);
        
        Assert.Equivalent(expectedGeo, retrievedInput.Details.GeoCoordinates.GetValueOrThrow());
    }

    [Fact]
    public async Task AddAsync_And_GetAllAsync_CorrectlyAddAndReturnsInBulk_MultipleValidInputs()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var inputRepo = _services.GetRequiredService<ITlgInputsRepository>();
        
        var tlgInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(roleSetting: TestOriginatorRoleSetting.IntegrationTestDefault),
            inputGenerator.GetValidTlgInputTextMessage(roleSetting: TestOriginatorRoleSetting.IntegrationTestDefault),
            inputGenerator.GetValidTlgInputTextMessage(roleSetting: TestOriginatorRoleSetting.IntegrationTestDefault)
        };
        
        await inputRepo.AddAsync(tlgInputs);
        var retrievedInputs = await inputRepo.GetAllAsync(TlgAgent_PrivateChat_Default);
        await inputRepo.HardDeleteAllAsync(TlgAgent_PrivateChat_Default);

        Assert.Equivalent(tlgInputs, retrievedInputs);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIdNotExist()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITelegramUpdateGenerator>();
        var inputRepo = _services.GetRequiredService<ITlgInputsRepository>();
        
        var tlgAgent = new TlgAgent(
            inputGenerator.Randomizer.GenerateRandomLong(),
            TestUserAndChatId01_PrivateChat_Default,
            Operations);
    
        var retrievedInputs = await inputRepo.GetAllAsync(tlgAgent);
    
        Assert.Empty(retrievedInputs);
    }

    /* Main purpose is to verify that the Details column doesn't have values with outdated schema e.g. because
    its migration has been forgotten after the details schema evolved in the model/code. */ 
    // [Theory(Skip = "Waiting to migrate the old DB data")]
    // [Theory(Skip = "Running tests from unknown IP / internet")]
    [Theory]
    [InlineData(TestUser_DanielGorin_TelegramId, false)]
    [InlineData(TestUser_DanielGorin_TelegramId, true)]
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
        
        var inputRepo = _services.GetRequiredService<ITlgInputsRepository>();
        
        // No assert needed: test fails when exception thrown!
        await inputRepo.GetAllAsync(new TlgAgent(devDbUserId, devDbUserId.Id, Operations));
        await inputRepo.GetAllAsync(new TlgAgent(devDbUserId, devDbUserId.Id, Communications));
        await inputRepo.GetAllAsync(new TlgAgent(devDbUserId, devDbUserId.Id, Notifications));
    }
}