using CheckMade.Common.Interfaces.Utils;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CheckMade.Telegram.Tests.Integration;

public class MessageRepositoryTests(ITestOutputHelper testOutputHelper)
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task MessageRepository_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var fakeInputMessage = utils.GetValidModelInputTextMessage();
        var messageRepo = _services.GetRequiredService<IMessageRepository>();
        
        var expectedRetrieval = new List<InputMessage>
        {
            new (fakeInputMessage.UserId, fakeInputMessage.Details)
        };
        
        // Act
        await messageRepo.AddAsync(fakeInputMessage);
    
        var retrievedMessages = 
            (await messageRepo.GetAllAsync(fakeInputMessage.UserId))
            .OrderByDescending(x => x.Details.TelegramDate)
            .ToList().AsReadOnly();

        await messageRepo.HardDeleteAsync(fakeInputMessage.UserId);
        
        // Assert
        expectedRetrieval[0].Should().BeEquivalentTo(retrievedMessages[0]);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIdNotExist()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var randomizer = _services.GetRequiredService<IRandomizer>();
        var messageRepo = _services.GetRequiredService<IMessageRepository>();
        var userId = randomizer.GenerateRandomLong();
    
        // Act
        var retrievedMessages = await messageRepo.GetAllAsync(userId);
    
        // Assert
        retrievedMessages.Should().BeEmpty();
    }

    // This test runs on whichever the 'current DB' is, i.e. local db when executed from dev machine.
    // Its main purpose is to verify that the Details column doesn't have values with outdated schema e.g. because
    // its migration has been forgotten after the details schema evolved in the model. 
    [Fact]
    public async Task Verifies_CurrentDb_DoesNotHaveInvalidData()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const long devDbUserId = 215737196L; // Daniel's Telegram ID
        var messageRepo = _services.GetRequiredService<IMessageRepository>();
        
        // Act
        Func<Task<IEnumerable<InputMessage>>> getAllAction = async () => await messageRepo.GetAllAsync(devDbUserId);

        // Assert 
        await getAllAction.Should().NotThrowAsync<DataAccessException>();
    }
}