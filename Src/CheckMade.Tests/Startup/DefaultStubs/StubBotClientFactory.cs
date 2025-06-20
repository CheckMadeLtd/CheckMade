using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Bot.Telegram.BotClient;
using Moq;

namespace CheckMade.Tests.Startup.DefaultStubs;

internal sealed class StubBotClientFactory(Mock<IBotClientWrapper> mockBotClientWrapper) : IBotClientFactory
{
    public IBotClientWrapper CreateBotClient(InteractionMode interactionMode)
    {
        // CAREFUL PITFALL: DO NOT setup interactionMode-dependent return values here (e.g. MyInteractionMode)!
        // Explanation:
        
        /* While production code uses a dictionary of botClientsByInteractionMode, test code currently only uses a single
        mockBotClient/stubBotClient, without distinction by type - its interactionMode-independent behavior is specified in
        UnitTestStartup. If we wanted to be able to test for interactionMode-dependent properties of botClient in test-code,
        a major refactoring would be necessary, e.g. not mocking a single, scoped botClient but rather the entire
        botClientByInteractionMode dictionary. See also explanation about Mock<IBotClientWrapper> in UnitTestStartup! */ 
        
        return mockBotClientWrapper.Object;
    }
}
