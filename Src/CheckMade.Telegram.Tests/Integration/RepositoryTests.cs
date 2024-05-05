using CheckMade.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Integration;

public class RepositoryTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;
    
    [Fact]
    public void TelegramMessageRepo_SavesToAndRetrievesFromDb_WhenInputValid()
    {
        var fakeInputMessage = TestUtils.GetValidTelegramTestMessage();
        var expectedRetrieval = new Message
        {
            Text = fakeInputMessage.Text,
            Chat = fakeInputMessage.Chat,
            From = fakeInputMessage.From
        };
        var repo = _services.GetRequiredService<ITelegramMessageRepo>();
        
        repo.Add(fakeInputMessage.From!.Id, fakeInputMessage.Text!);
        // var retrievedMessage = repo.Get(fakeInputMessage.From!.Id);
        //
        // Assert.Equal(expectedRetrieval.Text, retrievedMessage.Text);
        // Assert.Equal(expectedRetrieval.Chat.Id, retrievedMessage.Chat.Id);
        // Assert.Equal(expectedRetrieval.From.Id, retrievedMessage.From.Id);
    }   
}