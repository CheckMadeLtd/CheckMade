using CheckMade.Telegram.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Integration;

public class RepositoryTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;
    
    [Fact]
    public void TelegramMessageRepo_SavesAndRetrievesOneMessage_WhenInputValid()
    {
        var fakeInputMessage = TestUtils.GetValidTestMessage();
        var expectedRetrieval = new List<Message>
        {
            new Message
            {
                Text = fakeInputMessage.Text,
                // Chat = fakeInputMessage.Chat,
                From = fakeInputMessage.From
            }
        };
        var repo = _services.GetRequiredService<IMessageRepo>();
        
        repo.Add(fakeInputMessage);
    
        var retrievedMessages = repo.GetAll(fakeInputMessage.From!.Id).ToList().AsReadOnly();
        
        Assert.Equal(expectedRetrieval[0].Text, retrievedMessages[0].Text);
        // Assert.Equal(expectedRetrieval[0].Chat.Id, retrievedMessages[0].Chat.Id);
        Assert.Equal(expectedRetrieval[0].From!.Id, retrievedMessages[0].From!.Id);
    }   
}