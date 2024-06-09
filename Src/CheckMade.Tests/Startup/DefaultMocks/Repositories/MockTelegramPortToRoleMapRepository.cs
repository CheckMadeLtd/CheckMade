using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories;

internal class MockTelegramPortToRoleMapRepository : ITelegramPortToRoleMapRepository
{
    public Task<IEnumerable<TelegramPortToRoleMap>> GetAllAsync()
    {
        var builder = ImmutableArray.CreateBuilder<TelegramPortToRoleMap>();
        
        // #1
        
        builder.Add(new TelegramPortToRoleMap(
            TestUtils.SanitaryOpsAdmin1, 
            new TelegramPort(ITestUtils.TestUserId_01, ITestUtils.TestChatId_01),
            DateTime.Now));
        
        builder.Add(new TelegramPortToRoleMap(
            TestUtils.SanitaryOpsInspector1, 
            new TelegramPort(ITestUtils.TestUserId_01, ITestUtils.TestChatId_02),
            DateTime.Now));
        
        builder.Add(new TelegramPortToRoleMap(
            TestUtils.SanitaryOpsEngineer1, 
            new TelegramPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_03),
            DateTime.Now));
        
        builder.Add(new TelegramPortToRoleMap(
            TestUtils.SanitaryOpsCleanLead1, 
            new TelegramPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_04),
            DateTime.Now));
        
        builder.Add(new TelegramPortToRoleMap(
            TestUtils.SanitaryOpsObserver1, 
            new TelegramPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_05),
            DateTime.Now));
        
        // #2
        
        builder.Add(new TelegramPortToRoleMap(
            TestUtils.SanitaryOpsEngineer2, 
            new TelegramPort(ITestUtils.TestUserId_03 , ITestUtils.TestChatId_06),
            DateTime.Now));
        
        builder.Add(new TelegramPortToRoleMap(
            TestUtils.SanitaryOpsCleanLead2, 
            new TelegramPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_07),
            DateTime.Now));
        
        return Task.FromResult<IEnumerable<TelegramPortToRoleMap>>(builder.ToImmutable());
    }
}