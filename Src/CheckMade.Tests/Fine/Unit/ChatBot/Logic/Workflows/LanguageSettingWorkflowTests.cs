using System.Diagnostics.CodeAnalysis;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.LanguageSettingWorkflow;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

public class LanguageSettingWorkflowTests
{
    private ServiceProvider? _services;
    
    [Theory]
    // Establishes that 'Settings' is always the same int for all InteractionModes / Bots.
    [InlineData((int)OperationsBotCommands.Settings)]
    [InlineData((int)CommunicationsBotCommands.Settings)]
    [InlineData((int)NotificationsBotCommands.Settings)]
    [SuppressMessage("Usage", "xUnit1025:InlineData should be unique within the Theory it belongs to")]
    public void DetermineCurrentState_ReturnsInitial_WhenLastInputWasBotCommand(int botCommand)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = TlgAgent_PrivateChat_Default;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput>
        {
            utils.GetValidTlgInputTextMessage(),
            utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, botCommand)
        };
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(
            States.Initial, 
            actualState);
    }
    
    [Fact]
    public void DetermineCurrentState_ReturnsReceived_WhenLastInputWasCallbackQuery()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = TlgAgent_PrivateChat_Default;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput>
        {
            utils.GetValidTlgInputTextMessage(),
            utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.Settings),
            utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de))
        };
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(
            States.ReceivedLanguageSetting,
            actualState);
    }
    
    [Fact]
    public void DetermineCurrentState_ReturnsCompleted_WhenLastInputIsAfterCallbackQueryInput()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = TlgAgent_PrivateChat_Default;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput>
        {
            utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.Settings),
            utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
            utils.GetValidTlgInputTextMessage()
        }; 
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(
            States.Completed,
            actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ShowsLanguageSelectionMenu_InInitialState()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = TlgAgent_PrivateChat_Default;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputSettingsCommand = utils.GetValidTlgInputCommandMessage(
            Operations,
            (int)OperationsBotCommands.Settings); 
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(text: "random other irrelevant to workflow"),
                inputSettingsCommand
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();

        var actualOutput = await workflow.GetNextOutputAsync(inputSettingsCommand);
        
        Assert.Equal(
            "🌎 Please select your preferred language:", 
            GetFirstRawEnglish(actualOutput));
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
        var tlgAgent = TlgAgent_PrivateChat_Default;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputSettingsCommand = utils.GetValidTlgInputCommandMessage(
            Operations, 
            (int)OperationsBotCommands.Settings); 
        var languageSettingInput = utils.GetValidTlgInputCallbackQueryForDomainTerm(
            Dt(languageCode));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputTextMessage(text: "random decoy irrelevant to workflow"),
                inputSettingsCommand,
                utils.GetValidTlgInputTextMessage(text: "random decoy irrelevant to workflow"),
                languageSettingInput
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockUserRepo = _services.GetRequiredService<Mock<IUsersRepository>>();
        var workflow = _services.GetRequiredService<ILanguageSettingWorkflow>();

        var actualOutput = await workflow.GetNextOutputAsync(languageSettingInput);
        
        Assert.StartsWith(
            "New language: ",
            GetFirstRawEnglish(actualOutput));
        
        mockUserRepo.Verify(x => x.UpdateLanguageSettingAsync(
            RoleBindFor_SanitaryOpsAdmin_Default.Role.User,
            languageCode));
    }
}