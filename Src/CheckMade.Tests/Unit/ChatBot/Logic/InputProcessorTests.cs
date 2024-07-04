using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public class InputProcessorTests
{
    private ServiceProvider? _services;
 
    [Fact]
    public async Task ProcessInputAsync_WelcomesAndPromptsAuthentication_ForStartCommandOfUnauthenticatedUser()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();

        var startCommand = inputGenerator.GetValidTlgInputCommandMessage(
            UserId02_ChatId03_Operations.Mode,
            TlgStart.CommandCode,
            UserId02_ChatId03_Operations.UserId,
            UserId02_ChatId03_Operations.ChatId,
            roleSetting: TestOriginatorRoleSetting.None);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: new[] { startCommand });
        _services = services;
        
        var inputProcessor = _services.GetRequiredService<IInputProcessor>();
        
        var expectedOutputs = new List<OutputDto>
        {
            new(){ Text = Ui("🫡 Welcome to the CheckMade ChatBot. I shall follow your command!") },
            new(){ Text = UserAuthWorkflow.EnterTokenPrompt.Text.GetValueOrThrow() }
        };
        
        var actualOutput = 
            await inputProcessor
                .ProcessInputAsync(startCommand);

        Assert.Equivalent(
            expectedOutputs,
            actualOutput);
    }
    
    [Fact]
    public async Task ProcessInputAsync_ReturnsWarning_ForCallbackQuery_ToOutOfScopeInlineKeyboardButtonClick()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        // Back to messageId: 4 because user clicked on button in chat history, but /settings now out-of-scope
        var outOfScopeCallbackQuery =
            inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.en), 
                messageId: 4);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: new[]
            {
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.Settings,
                    messageId: 2),
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de),
                    messageId: 4),
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.NewIssue,
                    messageId: 6),
                outOfScopeCallbackQuery
            });
        _services = services;
        
        const string expectedWarningOutput = 
            "The previous workflow was completed, so your last message/action will be ignored.";
        var inputProcessor = _services.GetRequiredService<IInputProcessor>();

        var actualOutput = 
            await inputProcessor
                .ProcessInputAsync(outOfScopeCallbackQuery);
        
        Assert.Equal(
            expectedWarningOutput,
            TestUtils.GetFirstRawEnglish(actualOutput));
    }

    [Fact]
    public async Task ProcessInputAsync_PrefixesWarning_WhenUserInterruptedPreviousWorkflow_WithNewBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var interruptingBotCommandInput =
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                (int)OperationsBotCommands.NewAssessment); 

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: new[]
            {
                // Decoys
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                // Relevant
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.Settings),
                interruptingBotCommandInput
            });
        _services = services;
        
        const string expectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        var inputProcessor = _services.GetRequiredService<IInputProcessor>();
        
        var actualOutput =
            await inputProcessor
                .ProcessInputAsync(interruptingBotCommandInput);
        
        Assert.Equal(
            expectedWarningOutput,
            TestUtils.GetFirstRawEnglish(actualOutput));
    }
    
    [Fact]
    public async Task? ProcessInputAsync_NoWarning_ForNewBotCommand_WhenUserCompletedPreviousWorkflow()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var notInterruptingBotCommandInput =
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                (int)OperationsBotCommands.NewAssessment); 

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: new[]
            {
                inputGenerator.GetValidTlgInputCommandMessage(
                    tlgAgent.Mode,
                    (int)OperationsBotCommands.Settings),
                inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(
                    Dt(LanguageCode.de)),
                notInterruptingBotCommandInput
            });
        _services = services;
        
        const string notExpectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        var inputProcessor = _services.GetRequiredService<IInputProcessor>();
        
        var actualOutput = 
            await inputProcessor
                .ProcessInputAsync(notInterruptingBotCommandInput);
        
        Assert.NotEqual(
            notExpectedWarningOutput,
            TestUtils.GetFirstRawEnglish(actualOutput));
    }
}