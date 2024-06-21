using System.Diagnostics.CodeAnalysis;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.LanguageSettingWorkflow;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using static CheckMade.Tests.ITestUtils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static CheckMade.Tests.TestData;

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
        var tlgAgent = new TlgAgent(TestUserId_01, TestUserId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(tlgAgent.UserId, tlgAgent.ChatId),
                utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, botCommand)
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(tlgAgent);
        
        Assert.Equal(States.Initial, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsReceived_WhenLastInputWasCallbackQuery()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(userId: tlgAgent.UserId, chatId: tlgAgent.ChatId),
                utils.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode, (int)OperationsBotCommands.Settings,
                tlgAgent.UserId, tlgAgent.ChatId),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de),
                    tlgAgent.UserId, tlgAgent.ChatId)
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(tlgAgent);
        
        Assert.Equal(States.ReceivedLanguageSetting, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsCompleted_WhenLastInputIsAfterCallbackQueryInput()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestUserId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode, (int)OperationsBotCommands.Settings,
                    tlgAgent.UserId, tlgAgent.ChatId),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de),
                    tlgAgent.UserId, tlgAgent.ChatId),
                utils.GetValidTlgInputTextMessage(userId: tlgAgent.UserId, chatId: tlgAgent.ChatId)
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(tlgAgent);
        
        Assert.Equal(States.Completed, actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ShowsLanguageSelectionMenu_InInitialState()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestUserId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputSettingsCommand = utils.GetValidTlgInputCommandMessage(
            Operations, (int)OperationsBotCommands.Settings, 
            tlgAgent.UserId, tlgAgent.ChatId); 
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(text: "random other irrelevant to workflow", 
                    userId: tlgAgent.UserId, chatId: tlgAgent.ChatId),
                inputSettingsCommand
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();

        var actualOutput = await workflow.GetNextOutputAsync(inputSettingsCommand);
        
        Assert.Equal("🌎 Please select your preferred language:", GetFirstRawEnglish(actualOutput));
    }
    
    [Theory]
    [InlineData(LanguageCode.en)]
    [InlineData(LanguageCode.de)]
    public async Task GetNextOutputAsync_ReturnsSuccessMessage_AndSavesNewLanguageSetting_WhenLanguageChosen(
        LanguageCode languageCode)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputSettingsCommand = utils.GetValidTlgInputCommandMessage(
            Operations, (int)OperationsBotCommands.Settings, 
            tlgAgent.UserId, tlgAgent.ChatId); 
        var languageSettingInput = utils.GetValidTlgInputCallbackQueryForDomainTerm(
            Dt(languageCode), 
            tlgAgent.UserId, tlgAgent.ChatId);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(text: "random other irrelevant to workflow", 
                    userId: tlgAgent.UserId, chatId: tlgAgent.ChatId),
                inputSettingsCommand,
                utils.GetValidTlgInputTextMessage(text: "random other irrelevant to workflow", 
                    userId: tlgAgent.UserId, chatId: tlgAgent.ChatId),
                languageSettingInput
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockUserRepo = _services.GetRequiredService<Mock<IUserRepository>>();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();

        var actualOutput = await workflow.GetNextOutputAsync(languageSettingInput);
        
        Assert.StartsWith("New language: ", GetFirstRawEnglish(actualOutput));
        
        mockUserRepo.Verify(x => x.UpdateLanguageSettingAsync(
            TestUserDaniel,
            languageCode));
    }
}