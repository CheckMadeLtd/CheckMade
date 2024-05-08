using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using Moq;

namespace CheckMade.Telegram.Tests.DefaultMocks;

internal class MockBotClientFactory(Mock<IBotClientFactory> mockBotClientFactory) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(BotType botType)
    {
        /* In the future, when we need to test different behaviours of botClient for different botType, we can
         then set up different behaviours for the mockBotClient as a function of the given botType */ 
        
        var mockBotClient = new Mock<IBotClientWrapper>();

        return mockBotClient.Object;
    }
}
