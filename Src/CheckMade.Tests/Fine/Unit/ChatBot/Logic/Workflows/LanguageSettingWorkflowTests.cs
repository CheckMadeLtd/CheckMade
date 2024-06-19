using CheckMade.ChatBot.Logic.Workflows.Concrete;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.LanguageSettingWorkflow;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Tests.Startup;
using static CheckMade.Tests.ITestUtils;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

public class LanguageSettingWorkflowTests
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsInitial_WhenLastInputWasBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01, TestChatId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(),
                utils.GetValidTlgInputCommandMessage(
                    InteractionMode.Operations,
                    (int)OperationsBotCommands.Settings)
            });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();

        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync();
        
        Assert.Equal(States.Initial, actualState);
    }
}