using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories;

internal class MockChatIdByOutputDestinationRepository : IChatIdByOutputDestinationRepository
{
    public Task<IEnumerable<RoleBotTypeToChatIdMapping>> GetAllOrThrowAsync()
    {
        var builder = ImmutableArray.CreateBuilder<RoleBotTypeToChatIdMapping>();
        
        // #1
        
        // Using chat groups (i.e. different ChatId per BotType)
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsAdmin1, BotType.Operations, ITestUtils.TestChatId_01));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsAdmin1, BotType.Communications, ITestUtils.TestChatId_02));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsAdmin1, BotType.Notifications, ITestUtils.TestChatId_03));
        
        // Not using chat groups (i.e. same ChatId (=UserId) for each BotType)
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsInspector1, BotType.Operations, ITestUtils.TestChatId_04));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsInspector1, BotType.Communications, ITestUtils.TestChatId_04));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsInspector1, BotType.Notifications, ITestUtils.TestChatId_04));
        
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsEngineer1, BotType.Operations, ITestUtils.TestChatId_05));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsEngineer1, BotType.Communications, ITestUtils.TestChatId_05));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsEngineer1, BotType.Notifications, ITestUtils.TestChatId_05));
        
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsCleanLead1, BotType.Operations, ITestUtils.TestChatId_06));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsCleanLead1, BotType.Communications, ITestUtils.TestChatId_06));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsCleanLead1, BotType.Notifications, ITestUtils.TestChatId_06));
        
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsObserver1, BotType.Operations, ITestUtils.TestChatId_07));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsObserver1, BotType.Communications, ITestUtils.TestChatId_07));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsObserver1, BotType.Notifications, ITestUtils.TestChatId_07));
        
        // #2
        
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsEngineer2, BotType.Operations, ITestUtils.TestChatId_08));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsEngineer2, BotType.Communications, ITestUtils.TestChatId_08));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsEngineer2, BotType.Notifications, ITestUtils.TestChatId_08));
        
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsCleanLead2, BotType.Operations, ITestUtils.TestChatId_09));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsCleanLead2, BotType.Communications, ITestUtils.TestChatId_09));
        builder.Add(new RoleBotTypeToChatIdMapping(
            TestUtils.SanitaryOpsCleanLead2, BotType.Notifications, ITestUtils.TestChatId_09));
        
        return Task.FromResult<IEnumerable<RoleBotTypeToChatIdMapping>>(builder.ToImmutable());
    }
}