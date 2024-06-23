using System.Collections.Immutable;
using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
using CheckMade.Tests.Startup.DefaultMocks;
using CheckMade.Tests.Startup.DefaultMocks.Repositories.ChatBot;
using CheckMade.Tests.Startup.DefaultMocks.Repositories.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using File = Telegram.Bot.Types.File;
using static CheckMade.Tests.TestData;

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
        RegisterBotClientMocks();
        RegisterExternalServicesMocks();
        RegisterPersistenceMocks();
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

    private void RegisterExternalServicesMocks()
    {
        Services.AddScoped<IBlobLoader, MockBlobLoader>();
        Services.AddScoped<IHttpDownloader, MockHttpDownloader>(); 
    }

    private void RegisterPersistenceMocks()
    {
        Services.AddScoped<ITlgInputsRepository, MockTlgInputsRepository>(_ => 
            new MockTlgInputsRepository(new Mock<ITlgInputsRepository>()));

        Services.AddScoped<ILiveEventSeriesRepository, MockLiveEventSeriesRepository>();
        
        Services.AddScoped<IRolesRepository, MockRolesRepository>();

        var mockUserRepo = new Mock<IUsersRepository>();
        Services.AddScoped<IUsersRepository>(_ => mockUserRepo.Object);
        Services.AddScoped<Mock<IUsersRepository>>(_ => mockUserRepo);
        
        var mockTlgAgentRoleRepo = new Mock<ITlgAgentRoleBindingsRepository>();

        mockTlgAgentRoleRepo
            .Setup(arb => arb.GetAllAsync())
            .ReturnsAsync(GetTestingTlgAgentRoles());

        Services.AddScoped<ITlgAgentRoleBindingsRepository>(_ => mockTlgAgentRoleRepo.Object);
        Services.AddScoped<Mock<ITlgAgentRoleBindingsRepository>>(_ => mockTlgAgentRoleRepo);
    }

    private static ImmutableArray<TlgAgentRoleBind> GetTestingTlgAgentRoles()
    {
        var builder = ImmutableArray.CreateBuilder<TlgAgentRoleBind>();

        // #1
        
        builder.Add(new TlgAgentRoleBind(
            DanielIsSanitaryOpsAdminAtMockParooka2024, 
            new TlgAgent(TestUserId_01, TestChatId_01, Operations),
            DateTime.UtcNow, Option<DateTime>.None()));
        
        // Group: same Role & TlgAgent - all three InteractionModes
        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsInspector1, 
            new TlgAgent(TestUserId_01, TestChatId_02, Operations),
            DateTime.UtcNow, Option<DateTime>.None()));
        
        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsInspector1, 
            new TlgAgent(TestUserId_01, TestChatId_02, Communications),
            DateTime.UtcNow, Option<DateTime>.None()));

        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsInspector1, 
            new TlgAgent(TestUserId_01, TestChatId_02, Notifications),
            DateTime.UtcNow, Option<DateTime>.None()));


        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsEngineer1, 
            new TlgAgent(TestUserId_02, TestChatId_03, Operations),
            DateTime.UtcNow, Option<DateTime>.None()));
        
        // Expired on purpose - for Unit Tests!
        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsEngineer1, 
            new TlgAgent(TestUserId_02, TestChatId_03, Operations),
            new DateTime(1999, 01, 01), 
            new DateTime(1999, 02, 02), 
            DbRecordStatus.Historic));

        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsCleanLead1German, 
            new TlgAgent(TestUserId_02, TestChatId_04, Operations),
            DateTime.UtcNow, Option<DateTime>.None()));
        
        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsObserver1, 
            new TlgAgent(TestUserId_03, TestChatId_05, Operations),
            DateTime.UtcNow, Option<DateTime>.None()));
        
        // #2
        
        // Used in Unit Test 'GetNextOutputAsync_CreatesTlgAgentRolesForMissingMode_WhenValidTokenSubmitted_FromPrivateChat'
        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsEngineer2, 
            new TlgAgent(TestUserId_03 , TestChatId_06, Communications),
            DateTime.UtcNow, Option<DateTime>.None()));
        
        builder.Add(new TlgAgentRoleBind(
            SanitaryOpsCleanLead2English, 
            new TlgAgent(TestUserId_03, TestChatId_07, Operations),
            DateTime.UtcNow, Option<DateTime>.None()));
        
        // No TlgAgentRoleBind for role 'Inspector2' on purpose for Unit Test, e.g.
        // GetNextOutputAsync_CreatesTlgAgentRole_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup

        return builder.ToImmutable();
    }
}
