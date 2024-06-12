using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Utils;
using Moq;

namespace CheckMade.Tests.Startup.DefaultMocks.Repositories.Tlg;

internal class MockTlgClientPortRoleRepository : Mock<ITlgClientPortRoleRepository>
{
    public Task<IEnumerable<TlgClientPortRole>> GetAllAsync()
    {
        var builder = ImmutableArray.CreateBuilder<TlgClientPortRole>();
        
        // #1
        
        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsAdmin1, 
            new TlgClientPort(ITestUtils.TestUserId_01, ITestUtils.TestChatId_01),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsInspector1, 
            new TlgClientPort(ITestUtils.TestUserId_01, ITestUtils.TestChatId_02),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsEngineer1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_03),
            DateTime.Now, Option<DateTime>.None()));
        
        // Expired on purpose - for Unit Tests!
        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsEngineer1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_03),
            new DateTime(1999, 01, 01), new DateTime(1999, 02, 02), 
            DbRecordStatus.Historic));

        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsCleanLead1, 
            new TlgClientPort(ITestUtils.TestUserId_02, ITestUtils.TestChatId_04),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsObserver1, 
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_05),
            DateTime.Now, Option<DateTime>.None()));
        
        // #2
        
        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsEngineer2, 
            new TlgClientPort(ITestUtils.TestUserId_03 , ITestUtils.TestChatId_06),
            DateTime.Now, Option<DateTime>.None()));
        
        builder.Add(new TlgClientPortRole(
            ITestUtils.SanitaryOpsCleanLead2, 
            new TlgClientPort(ITestUtils.TestUserId_03, ITestUtils.TestChatId_07),
            DateTime.Now, Option<DateTime>.None()));
        
        // No TlgClientPortRole for role 'Inspector2' on purpose - for Unit Tests!
        
        return Task.FromResult<IEnumerable<TlgClientPortRole>>(builder.ToImmutable());
    }
}