using System.Collections.Immutable;
using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup.DefaultMocks;
using CheckMade.Tests.Startup.DefaultMocks.Repositories.ChatBot;
using CheckMade.Tests.Startup.DefaultMocks.Repositories.Core;
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
            .Setup(tarb => tarb.GetAllAsync())
            .ReturnsAsync(GetTestingTlgAgentRoleBindings());

        mockTlgAgentRoleRepo
            .Setup(tarb => tarb.GetAllActiveAsync())
            .ReturnsAsync(GetTestingTlgAgentRoleBindings().Where(tarb => tarb.Status == DbRecordStatus.Active));

        Services.AddScoped<ITlgAgentRoleBindingsRepository>(_ => mockTlgAgentRoleRepo.Object);
        Services.AddScoped<Mock<ITlgAgentRoleBindingsRepository>>(_ => mockTlgAgentRoleRepo);
    }

    private static ImmutableArray<TlgAgentRoleBind> GetTestingTlgAgentRoleBindings()
    {
        var builder = ImmutableArray.CreateBuilder<TlgAgentRoleBind>();
        
        builder.Add(RoleBindFor_SanitaryOpsAdmin_Default);
        
        builder.Add(RoleBindFor_SanitaryOpsInspector1_InPrivateChat_OperationsMode);
        builder.Add(RoleBindFor_SanitaryOpsInspector1_InPrivateChat_CommunicationsMode);
        builder.Add(RoleBindFor_SanitaryOpsInspector1_InPrivateChat_NotificationsMode);
        
        builder.Add(RoleBindFor_SanitaryOpsEngineer1_HistoricOnly);
        builder.Add(RoleBindFor_SanitaryOpsCleanLead1_German);
        builder.Add(RoleBindFor_SanitaryOpsEngineer2_OnlyCommunicationsMode);
        
        builder.Add(RoleBindFor_SanitaryOpsObserver1);
        builder.Add(RoleBindFor_SanitaryOpsCleanLead2);
        
        return builder.ToImmutable();
    }
}
