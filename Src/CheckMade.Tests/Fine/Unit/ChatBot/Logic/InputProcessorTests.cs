using CheckMade.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic;

public class InputProcessorTests
{
    private ServiceProvider? _services;
 
    // [Fact]
    // public async Task ProcessInputAsync_WelcomesAndPromptsAuthentication_ForStartCommandOfUnauthenticatedUser()
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     var utils = _services.GetRequiredService<ITestUtils>();
    //     var serviceCollection = new UnitTestStartup().Services;
    //     
    //     // ToDo: replace this and in other unit tests with new tlgAgentWithoutBind
    //     var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
    //     var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
    //     
    //     // var startCommand = 
    // }
    
    [Fact]
    public async Task ProcessInputAsync_ReturnsWarning_ForCallbackQuery_ToOutOfScopeInlineKeyboardButtonClick()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var serviceCollection = new UnitTestStartup().Services;
        
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        // Back to messageId: 4 because user clicked on button in chat history, but /settings now out-of-scope
        var outOfScopeCallbackQuery =
            utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.en), 
                messageId: 4);
        
        mockTlgInputsRepo
            .Setup(x => x.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.Settings, 
                    messageId: 2),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de), 
                    messageId: 4),
                utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.NewIssue, 
                    messageId: 6),
                outOfScopeCallbackQuery
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        const string expectedWarningOutput = 
            "The previous workflow was completed, so your last message/action will be ignored.";
        var inputProcessor = _services.GetRequiredService<IInputProcessorFactory>().GetInputProcessor(tlgAgent.Mode);

        var actualOutput = await inputProcessor.ProcessInputAsync(outOfScopeCallbackQuery);
        
        Assert.Equal(expectedWarningOutput, GetFirstRawEnglish(actualOutput));
    }

    [Fact]
    public async Task ProcessInputAsync_PrefixesWarning_WhenUserInterruptedPreviousWorkflow_WithNewBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var serviceCollection = new UnitTestStartup().Services;
        
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var interruptingBotCommandInput =
            utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.NewIssue); 
        
        mockTlgInputsRepo
            .Setup(x => x.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.Settings),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.Settings),
                interruptingBotCommandInput
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        const string expectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        var inputProcessor = _services.GetRequiredService<IInputProcessorFactory>().GetInputProcessor(tlgAgent.Mode);

        var actualOutput = await inputProcessor
            .ProcessInputAsync(interruptingBotCommandInput);
        
        Assert.Equal(expectedWarningOutput, GetFirstRawEnglish(actualOutput));
    }
    
    [Fact]
    public async Task? ProcessInputAsync_NoWarning_ForNewBotCommand_WhenUserCompletedPreviousWorkflow()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var serviceCollection = new UnitTestStartup().Services;
        
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var notInterruptingBotCommandInput =
            utils.GetValidTlgInputCommandMessage(tlgAgent.Mode, (int)OperationsBotCommands.NewIssue); 
        
        mockTlgInputsRepo
            .Setup(x => x.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode, (int)OperationsBotCommands.Settings),
                utils.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.de)),
                notInterruptingBotCommandInput
            });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        const string notExpectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        var inputProcessor = _services.GetRequiredService<IInputProcessorFactory>().GetInputProcessor(tlgAgent.Mode);

        var actualOutput = await inputProcessor
            .ProcessInputAsync(notInterruptingBotCommandInput);
        
        Assert.NotEqual(notExpectedWarningOutput, GetFirstRawEnglish(actualOutput));
    }
}