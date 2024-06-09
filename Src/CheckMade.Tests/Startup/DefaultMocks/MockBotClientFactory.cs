using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Telegram.Function.Services.BotClient;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks;

internal class MockBotClientFactory(Mock<IBotClientWrapper> mockBotClientWrapper) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(TlgBotType botType)
    {
        // CAREFUL PITFALL: DO NOT setup botType-dependent return values here (e.g. MyBotType)! Explanation:
        
        /* While production code uses a dictionary of botClientsByBotType, test code currently only uses a single
        mockBotClient, without distinction by type - its botType-independent behaviour is specified in
        UnitTestStartup. If we wanted to be able to test for botType-dependent properties of botClient in test-code,
        a major refactoring would be necessary, e.g. not mocking a single, scoped botClient but rather the entire
        botClientByBotType dictionary. See also explanation about Mock<IBotClientWrapper> in UnitTestStartup! */ 
        
        return mockBotClientWrapper.Object;
    }
}
