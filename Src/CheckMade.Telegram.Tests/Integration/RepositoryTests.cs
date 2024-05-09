using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Integration;

public class RepositoryTests
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task MessageRepo_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var fakeInputMessage = TestUtils.GetValidTestMessage();
        var messageRepo = _services.GetRequiredService<IMessageRepo>();
        
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
        var messageRepo = _services.GetRequiredService<IMessageRepo>();
        long userId = new Random().Next(10000);

        // Act
        var retrievedMessages = await messageRepo.GetAllAsync(userId);

        // Assert
        retrievedMessages.Should().BeEmpty();
    }
}