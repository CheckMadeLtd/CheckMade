using System.Diagnostics.CodeAnalysis;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.LanguageSettingWorkflow;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
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
        var clientPort = new TlgClientPort(TestUserId_01, TestUserId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(clientPort.UserId, clientPort.ChatId))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(userId: clientPort.UserId, chatId: clientPort.ChatId),
                utils.GetValidTlgInputCommandMessage(clientPort.Mode, botCommand)
            });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(clientPort);
        
        Assert.Equal(States.Initial, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsReceived_WhenLastInputWasCallbackQuery()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var clientPort = new TlgClientPort(TestUserId_01, TestUserId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(clientPort.UserId, clientPort.ChatId))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(),
                utils.GetValidTlgInputCommandMessage(
                    clientPort.Mode, (int)OperationsBotCommands.Settings),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de))
            });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(clientPort);
        
        Assert.Equal(States.ReceivedLanguageSetting, actualState);
    }

    [Fact]
    public async Task GetNextOutputAsync_ShowsLanguageSelectionMenu_InInitialState()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var clientPort = new TlgClientPort(TestUserId_01, TestUserId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputSettingsCommand =
            utils.GetValidTlgInputCommandMessage(
                Operations, 
                (int)OperationsBotCommands.Settings,
                clientPort.UserId, clientPort.ChatId); 
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(clientPort.UserId, clientPort.ChatId))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(userId: clientPort.UserId, chatId: clientPort.ChatId),
                inputSettingsCommand
            });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();

        var actualOutput = await workflow.GetNextOutputAsync(inputSettingsCommand);
        
        Assert.Equal("🌎 Please select your preferred language:", GetFirstRawEnglish(actualOutput));
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ReturnsSuccessMessage_AndSavesNewLanguageSetting_WhenLanguageChosen()
    {
        
    }
}