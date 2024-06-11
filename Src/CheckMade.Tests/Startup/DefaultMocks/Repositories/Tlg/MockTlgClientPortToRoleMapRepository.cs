using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Utils;

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
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsInspector1, 
            new TlgClientPort(ITestUtils.TestUserId_01, ITestUtils.TestChatId_02),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsEngineer1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_03),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsEngineer1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_03),
            new DateTime(1999, 01, 01), new DateTime(1999, 02, 02), 
            DbRecordStatus.Historic));

        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsCleanLead1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_04),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsObserver1, 
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_05),
            DateTime.Now, Option<DateTime>.None()));
        
        // #2
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsEngineer2, 
            new TlgClientPort(ITestUtils.TestUserId_03 , ITestUtils.TestChatId_06),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortToRoleMap(
            TestUtils.SanitaryOpsCleanLead2, 
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_07),
            DateTime.Now, Option<DateTime>.None()));
        
        return Task.FromResult<IEnumerable<TlgClientPortToRoleMap>>(builder.ToImmutable());
    }
}