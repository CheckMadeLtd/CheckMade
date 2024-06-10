using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Tlg;

internal class MockTlgClientPortToRoleMapRepository : ITlgClientPortToRoleMapRepository
{
    public Task<IEnumerable<TlgClientPortToRoleMap>> GetAllAsync()
    {
        var builder = ImmutableArray.CreateBuilder<TlgClientPortToRoleMap>();
        
        // #1
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsAdmin1, 
            new TlgClientPort(ITestUtils.TestUserId_01, ITestUtils.TestChatId_01),
            DateTime.Now));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsInspector1, 
            new TlgClientPort(ITestUtils.TestUserId_01, ITestUtils.TestChatId_02),
            DateTime.Now));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsEngineer1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_03),
            DateTime.Now));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsCleanLead1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_04),
            DateTime.Now));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsObserver1, 
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_05),
            DateTime.Now));
        
        // #2
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsEngineer2, 
            new TlgClientPort(ITestUtils.TestUserId_03 , ITestUtils.TestChatId_06),
            DateTime.Now));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsCleanLead2, 
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_07),
            DateTime.Now));
        
        return Task.FromResult<IEnumerable<TlgClientPortToRoleMap>>(builder.ToImmutable());
    }
}