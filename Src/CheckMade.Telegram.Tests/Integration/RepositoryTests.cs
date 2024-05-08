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
    public async Task TelegramMessageRepo_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        _services = new IntegrationTestStartup().ServiceProvider;
        
        // Arrange
        var fakeInputMessage = TestUtils.GetValidTestMessage();
        
        var expectedRetrieval = new List<InputTextMessage>
        {
            new (fakeInputMessage.UserId, fakeInputMessage.Details)
        };
        
        var repo = _services.GetRequiredService<IMessageRepo>();
        
        // Act
        await repo.AddAsync(fakeInputMessage);
    
        var retrievedMessages = 
            (await repo.GetAllAsync(fakeInputMessage.UserId))
            .OrderByDescending(x => x.Details.TelegramDate)
            .ToList().AsReadOnly();
        
        // Assert
        expectedRetrieval[0].Should().BeEquivalentTo(retrievedMessages[0]);
    }   
}