using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

public class LogoutWorkflowTests
{
    private ServiceProvider? _services;

    // [Fact]
    // public async 
    //
    [Fact]
    public async Task GetNextOutputAsync_LogsOutAndReturnsConfirmation_AfterUserConfirmsLogoutIntention()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var confirmLogoutCommand = utils.GetValidTlgInputCallbackQueryForControlPrompts(ControlPrompts.Yes,
            tlgAgent.UserId, tlgAgent.ChatId);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputCommandMessage(Operations, (int)OperationsBotCommands.Settings,
                    tlgAgent.UserId, tlgAgent.ChatId),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                utils.GetValidTlgInputCommandMessage(Operations, (int)OperationsBotCommands.Logout,
                    tlgAgent.UserId, tlgAgent.ChatId),
                confirmLogoutCommand
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockRoleBindingsRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();
        var workflow = _services.GetRequiredService<ILogoutWorkflow>();
        const string expectedMessage = "💨 Logged out.";
        var expectedBindUpdated = (await mockRoleBindingsRepo.Object.GetAllAsync())
            .First(arb => arb.TlgAgent == tlgAgent &&
                          arb.Status == DbRecordStatus.Active);
        
        var actualOutput = await workflow.GetNextOutputAsync(confirmLogoutCommand);
        
        Assert.Equal(expectedMessage, GetFirstRawEnglish(actualOutput));
        
        mockRoleBindingsRepo.Verify(x => x.UpdateStatusAsync(
            expectedBindUpdated,
            DbRecordStatus.Historic));
    }
    
    
}