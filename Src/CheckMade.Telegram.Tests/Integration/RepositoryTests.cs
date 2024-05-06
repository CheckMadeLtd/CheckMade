using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Integration;

public class RepositoryTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;
    
    [Fact]
    public void TelegramMessageRepo_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        var fakeInputMessage = TestUtils.GetValidTestMessage();
        var expectedRetrieval = new List<InputTextMessage>
        {
            new (fakeInputMessage.UserId, fakeInputMessage.Details)
        };
        var repo = _services.GetRequiredService<IMessageRepo>();
        
        repo.Add(fakeInputMessage);
    
        var retrievedMessages = 
            repo.GetAll(fakeInputMessage.UserId)
                .ToList().AsReadOnly();
        
        Assert.Equal(expectedRetrieval[0], retrievedMessages[0]);
    }   
}