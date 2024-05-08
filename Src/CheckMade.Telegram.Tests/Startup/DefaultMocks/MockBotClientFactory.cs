using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;

namespace CheckMade.Telegram.Tests.Startup.DefaultMocks;

internal class MockBotClientFactory(IBotClientWrapper mockBotClientWrapper) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(BotType botType)
    {
        /* In the future, when we need to test different behaviours of botClient for different botType, we can
         then set up different behaviours for the mockBotClient as a function of the given botType */ 
        
        return mockBotClientWrapper;
    }
}
