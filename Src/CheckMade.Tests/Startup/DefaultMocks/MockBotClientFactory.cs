using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Model;

namespace CheckMade.Tests.Startup.DefaultMocks;

internal class MockBotClientFactory(IBotClientWrapper mockBotClientWrapper) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClientOrThrow(BotType botType)
    {
        // since we are not using setup of any behaviour / return values, botType makes no difference at this point
        // (instead, in many tests, we simply 'verify' which behaviour was invoked on the mockBotClient)
        return mockBotClientWrapper;
    }
}
