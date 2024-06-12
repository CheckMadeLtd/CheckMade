using System.Collections.Immutable;
using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Utils;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Tests.Startup.DefaultMocks;
using CheckMade.Tests.Startup.DefaultMocks.Repositories.Core;
using CheckMade.Tests.Startup.DefaultMocks.Repositories.Tlg;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Tests.Startup;

[UsedImplicitly]
public class UnitTestStartup : TestStartupBase
{
    public UnitTestStartup()
    {
        RegisterServices();
    }

    protected override void RegisterTestTypeSpecificServices()
    {
        RegisterPersistenceMocks();
        RegisterBotClientMocks();
        RegisterExternalServicesMocks();        
    }

    private void RegisterBotClientMocks()
    {
        Services.AddScoped<IBotClientFactory, MockBotClientFactory>(sp => 
            new MockBotClientFactory(sp.GetRequiredService<Mock<IBotClientWrapper>>()));

        /* Adding Mock<IBotClientWrapper> into the D.I. container is necessary so that I can inject the same instance
         in my tests that is also used by the MockBotClientFactory below. This way I can verify behaviour on the
         mockBotClientWrapper without explicitly setting up the mock in the unit test itself.

         We choose 'AddScoped' because we want our dependencies scoped to the execution of each test method.
         That's why each test method creates its own ServiceProvider. That prevents:

         a) interference between test runs e.g. because of shared state in some dependency (which could e.g.
         falsify Moq's behaviour 'verifications'

         b) having two instanced of e.g. mockBotClientWrapper within a single test-run, when only one is expected
        */ 
        Services.AddScoped<Mock<IBotClientWrapper>>(_ =>
        {
            var mockBotClientWrapper = new Mock<IBotClientWrapper>();
            
            mockBotClientWrapper
                .Setup(x => x.GetFileAsync(It.IsNotNull<string>()))
                .ReturnsAsync(new File { FilePath = "fakeFilePath" });
            mockBotClientWrapper
                .Setup(x => x.MyBotToken)
                .Returns("fakeToken");
            
            return mockBotClientWrapper;
        });
    }

    private void RegisterPersistenceMocks()
    {
        Services.AddScoped<IRoleRepository, MockRoleRepository>();
        
        Services.AddScoped<ITlgInputRepository, MockTlgInputRepository>(_ => 
            new MockTlgInputRepository(new Mock<ITlgInputRepository>()));
        
        Services.AddScoped<Mock<ITlgClientPortRoleRepository>>(_ =>
        {
            var mockTlgClientPortRoleRepo = new Mock<ITlgClientPortRoleRepository>();

            mockTlgClientPortRoleRepo
                .Setup(cpr => cpr.GetAllAsync())
                .ReturnsAsync(GetTestingPortRoles());
            
            return mockTlgClientPortRoleRepo;
        });
    }

    private void RegisterExternalServicesMocks()
    {
        Services.AddScoped<IBlobLoader, MockBlobLoader>();
        Services.AddScoped<IHttpDownloader, MockHttpDownloader>(); 
    }
    
    private static ImmutableArray<TlgClientPortRole> GetTestingPortRoles()
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

        return builder.ToImmutable();
    }
}
