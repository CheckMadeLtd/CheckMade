using System.Diagnostics.CodeAnalysis;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.LanguageSettingWorkflow;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using static CheckMade.Tests.ITestUtils;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

[SuppressMessage("Usage", "xUnit1025:InlineData should be unique within the Theory it belongs to")]

public class LanguageSettingWorkflowTests
{
    private ServiceProvider? _services;
    
    [Theory]
    // Establishes that 'Settings' is always the same int for all InteractionModes / Bots.
    [InlineData((int)OperationsBotCommands.Settings)]
    [InlineData((int)CommunicationsBotCommands.Settings)]
    [InlineData((int)NotificationsBotCommands.Settings)]
    public async Task DetermineCurrentStateAsync_ReturnsInitial_WhenLastInputWasBotCommand(int botCommand)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        const long userAndChatId = TestUserId_01;
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(userAndChatId, userAndChatId))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(userId: userAndChatId, chatId: userAndChatId),
                utils.GetValidTlgInputCommandMessage(Operations, botCommand)
            });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(
            userAndChatId, userAndChatId, Operations);
        
        Assert.Equal(States.Initial, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsReceived_WhenLastInputWasCallbackQuery()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        const long userAndChatId = TestUserId_01;
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(userAndChatId, userAndChatId))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(),
                utils.GetValidTlgInputCommandMessage(
                    Operations, (int)OperationsBotCommands.Settings),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de))
            });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(
            userAndChatId, userAndChatId, Operations);
        
        Assert.Equal(States.ReceivedLanguageSetting, actualState);
    }
}