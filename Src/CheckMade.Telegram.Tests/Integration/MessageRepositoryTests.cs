using CheckMade.Common.Interfaces.Utils;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Integration;

public class MessageRepositoryTests
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task MessageRepository_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var fakeInputMessage = utils.GetValidTestMessage();
        var messageRepo = _services.GetRequiredService<IMessageRepository>();
        
        var expectedRetrieval = new List<InputTextMessage>
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
}