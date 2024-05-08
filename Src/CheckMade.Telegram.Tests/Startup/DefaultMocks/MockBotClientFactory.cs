using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;

namespace CheckMade.Telegram.Tests.Startup.DefaultMocks;

internal class MockBotClientFactory(IBotClientWrapper mockBotClientWrapper) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(BotType botType)
    {
        // since we are not using setup of any behaviour / return values, botType makes no difference at this point
        return mockBotClientWrapper;
    }
}
