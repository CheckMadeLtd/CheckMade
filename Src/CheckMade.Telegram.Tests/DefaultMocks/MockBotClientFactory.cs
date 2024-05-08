using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using Moq;

namespace CheckMade.Telegram.Tests.DefaultMocks;

internal class MockBotClientFactory(IMock<IBotClientFactory> mockBotClientFactory) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(BotType botType)
    {
        return botType switch
        {
            BotType.Submissions => mockBotClientFactory.Object.CreateBotClient(BotType.Submissions),
            BotType.Communications => mockBotClientFactory.Object.CreateBotClient(BotType.Communications),
            BotType.Notifications => mockBotClientFactory.Object.CreateBotClient(BotType.Notifications),
            _ => throw new ArgumentException("Invalid bot type")
        };
    }
}
