using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Integration;

public class RepositoryTests(IntegrationTestStartup setup) : IClassFixture<IntegrationTestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;
    
    [Fact]
    public async Task TelegramMessageRepo_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        var fakeInputMessage = TestUtils.GetValidTestMessage();
        
        var expectedRetrieval = new List<InputTextMessage>
        {
            new (fakeInputMessage.UserId, fakeInputMessage.Details)
        };
        var repo = _services.GetRequiredService<IMessageRepo>();
        
        await repo.AddAsync(fakeInputMessage);
    
        var retrievedMessages = 
            (await repo.GetAllAsync(fakeInputMessage.UserId))
            .OrderByDescending(x => x.Details.TelegramDate)
            .ToList().AsReadOnly();
        
        expectedRetrieval[0].Should().BeEquivalentTo(retrievedMessages[0]);
    }   
}