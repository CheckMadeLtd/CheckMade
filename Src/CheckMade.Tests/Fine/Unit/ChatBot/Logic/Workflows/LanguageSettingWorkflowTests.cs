using System.Diagnostics.CodeAnalysis;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.LanguageSettingWorkflow;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
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
    public void DetermineCurrentState_ReturnsInitial_WhenLastInputWasBotCommand(int botCommand)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId01_PrivateChat_Default, TestUserId01_PrivateChat_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput>
        {
            utils.GetValidTlgInputTextMessage(tlgAgent.UserId, tlgAgent.ChatId),
            utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, botCommand)
        };
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(States.Initial, actualState);
    }
    
    [Fact]
    public void DetermineCurrentState_ReturnsReceived_WhenLastInputWasCallbackQuery()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId01_PrivateChat_Default, TestChatId01_PrivateChat_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput>
        {
            utils.GetValidTlgInputTextMessage(userId: tlgAgent.UserId, chatId: tlgAgent.ChatId),
            utils.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, (int)OperationsBotCommands.Settings,
                tlgAgent.UserId, tlgAgent.ChatId),
            utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de),
                tlgAgent.UserId, tlgAgent.ChatId)
        };
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(States.ReceivedLanguageSetting, actualState);
    }
    
    [Fact]
    public void DetermineCurrentState_ReturnsCompleted_WhenLastInputIsAfterCallbackQueryInput()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId01_PrivateChat_Default, TestUserId01_PrivateChat_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput>
        {
            utils.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, (int)OperationsBotCommands.Settings,
                tlgAgent.UserId, tlgAgent.ChatId),
            utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de),
                tlgAgent.UserId, tlgAgent.ChatId),
            utils.GetValidTlgInputTextMessage(userId: tlgAgent.UserId, chatId: tlgAgent.ChatId)
        }; 
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(States.Completed, actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ShowsLanguageSelectionMenu_InInitialState()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId01_PrivateChat_Default, TestUserId01_PrivateChat_Default, Operations);
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
        var tlgAgent = new TlgAgent(TestUserId01_PrivateChat_Default, TestChatId01_PrivateChat_Default, Operations);
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
        var mockUserRepo = _services.GetRequiredService<Mock<IUsersRepository>>();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();

        var actualOutput = await workflow.GetNextOutputAsync(languageSettingInput);
        
        Assert.StartsWith("New language: ", GetFirstRawEnglish(actualOutput));
        
        mockUserRepo.Verify(x => x.UpdateLanguageSettingAsync(
            TestUser_Daniel,
            languageCode));
    }
}