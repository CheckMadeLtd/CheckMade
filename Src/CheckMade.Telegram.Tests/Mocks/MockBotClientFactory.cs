using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using Moq;
using Telegram.Bot;

namespace CheckMade.Telegram.Tests.Mocks;

public class MockBotClientFactory(IMock<IBotClientFactory> mockBotClientFactory) : IBotClientFactory
{
    public ITelegramBotClient CreateBotClient(BotType botType)
    {
        // ToDo: Verify that the provided botType here makes no difference.
        return mockBotClientFactory.Object.CreateBotClient(BotType.Submissions);
    }
}
