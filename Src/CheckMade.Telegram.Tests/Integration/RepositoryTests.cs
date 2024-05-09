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
        
        // Assert
        expectedRetrieval[0].Should().BeEquivalentTo(retrievedMessages[0]);
    }   
}