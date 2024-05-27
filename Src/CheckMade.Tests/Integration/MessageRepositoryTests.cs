using CheckMade.Common.Interfaces;
using CheckMade.Common.LangExt;
using CheckMade.Common.Persistence;
using CheckMade.Common.Utils.Generic;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model.DTOs;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CheckMade.Tests.Integration;

public class MessageRepositoryTests(ITestOutputHelper testOutputHelper)
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task MessageRepository_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var modelInputMessages = new[]
        {
            utils.GetValidModelInputTextMessageNoAttachment(),
            utils.GetValidModelInputTextMessageWithAttachment()
        };
        var messageRepo = _services.GetRequiredService<IMessageRepository>();

        foreach (var message in modelInputMessages)
        {
            var expectedRetrieval = new List<InputMessageDto>
            {
                new (message.UserId, message.ChatId, message.BotType, message.Details)
            };
        
            // Act
            await messageRepo.AddOrThrowAsync(message);
    
            var retrievedMessages = 
                (await messageRepo.GetAllOrThrowAsync(message.UserId))
                .OrderByDescending(x => x.Details.TelegramDate)
                .ToList().AsReadOnly();

            await messageRepo.HardDeleteAllOrThrowAsync(message.UserId);
        
            // Assert
            expectedRetrieval[0].Should().BeEquivalentTo(retrievedMessages[0]);
        }
    }

    [Fact]
    public async Task AddAsync_And_GetAllAsync_CorrectlyAddAndReturn_MultipleValidMessages()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var userId = utils.Randomizer.GenerateRandomLong();
        var modelInputMessages = new[]
        {
            utils.GetValidModelInputTextMessageNoAttachment(userId),
            utils.GetValidModelInputTextMessageNoAttachment(userId),
            utils.GetValidModelInputTextMessageNoAttachment(userId)
        };
        var messageRepo = _services.GetRequiredService<IMessageRepository>();
        
        // Act
        await messageRepo.AddOrThrowAsync(modelInputMessages);
        var retrievedMessages = await messageRepo.GetAllOrThrowAsync(userId);
        await messageRepo.HardDeleteAllOrThrowAsync(userId);

        // Assert
        retrievedMessages.Should().BeEquivalentTo(modelInputMessages);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIdNotExist()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var randomizer = _services.GetRequiredService<Randomizer>();
        var messageRepo = _services.GetRequiredService<IMessageRepository>();
        var userId = randomizer.GenerateRandomLong();
    
        // Act
        var retrievedMessages = await messageRepo.GetAllOrThrowAsync(userId);
    
        // Assert
        retrievedMessages.Should().BeEmpty();
    }

    /* Main purpose is to verify that the Details column doesn't have values with outdated schema e.g. because
    its migration has been forgotten after the details schema evolved in the model/code. */ 
    // [Theory(Skip = "Waiting to migrate the old DB data")]
    // [Theory(Skip = "Running tests from unknown IP / internet")]
    [Theory]
    [InlineData(TestUtils.TestUserDanielGorinTelegramId, false)]
    [InlineData(TestUtils.TestUserDanielGorinTelegramId, true)]
    public async Task Verifies_Db_DoesNotHaveInvalidTestData_ForGivenTestUser(
        long devDbUserId, bool overwriteDefaultDbConnProviderWithPrdDbConn)
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        if (overwriteDefaultDbConnProviderWithPrdDbConn)
        {
            var prdDbConnString = _services.GetRequiredService<PrdDbConnStringProvider>().Get;
            testOutputHelper.WriteLine(prdDbConnString);
            var serviceCollection = new IntegrationTestStartup().Services;
            serviceCollection.AddScoped<IDbConnectionProvider>(_ => new DbConnectionProvider(prdDbConnString));
            _services = serviceCollection.BuildServiceProvider();
        }
        
        var messageRepo = _services.GetRequiredService<IMessageRepository>();
        
        // Act
        Func<Task<IEnumerable<InputMessageDto>>> getAllAction = async () => 
            await messageRepo.GetAllOrThrowAsync(devDbUserId);
        
        // Assert 
        await getAllAction.Should().NotThrowAsync<DataAccessException>();
    }
}