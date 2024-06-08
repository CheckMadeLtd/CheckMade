using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories;

internal class MockTelegramUserChatDestinationToRoleMapRepository : ITelegramUserChatDestinationToRoleMapRepository
{
    public Task<IEnumerable<TelegramUserChatDestinationToRoleMap>> GetAllAsync()
    {
        var builder = ImmutableArray.CreateBuilder<TelegramUserChatDestinationToRoleMap>();
        
        // #1
        
        builder.Add(new TelegramUserChatDestinationToRoleMap(
            TestUtils.SanitaryOpsAdmin1, 
            new TelegramUserChatDestination(ITestUtils.TestUserId_01, ITestUtils.TestChatId_01)));
        
        builder.Add(new TelegramUserChatDestinationToRoleMap(
            TestUtils.SanitaryOpsInspector1, 
            new TelegramUserChatDestination(ITestUtils.TestUserId_01, ITestUtils.TestChatId_02)));
        
        builder.Add(new TelegramUserChatDestinationToRoleMap(
            TestUtils.SanitaryOpsEngineer1, 
            new TelegramUserChatDestination(ITestUtils.TestUserId_02, ITestUtils.TestChatId_03)));
        
        builder.Add(new TelegramUserChatDestinationToRoleMap(
            TestUtils.SanitaryOpsCleanLead1, 
            new TelegramUserChatDestination(ITestUtils.TestUserId_02, ITestUtils.TestChatId_04)));
        
        builder.Add(new TelegramUserChatDestinationToRoleMap(
            TestUtils.SanitaryOpsObserver1, 
            new TelegramUserChatDestination(ITestUtils.TestUserId_03, ITestUtils.TestChatId_05)));
        
        // #2
        
        builder.Add(new TelegramUserChatDestinationToRoleMap(
            TestUtils.SanitaryOpsEngineer2, 
            new TelegramUserChatDestination(ITestUtils.TestUserId_03 , ITestUtils.TestChatId_06)));
        
        builder.Add(new TelegramUserChatDestinationToRoleMap(
            TestUtils.SanitaryOpsCleanLead2, 
            new TelegramUserChatDestination(ITestUtils.TestUserId_03, ITestUtils.TestChatId_07)));
        
        return Task.FromResult<IEnumerable<TelegramUserChatDestinationToRoleMap>>(builder.ToImmutable());
    }
}