using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public class InputProcessorTests
{
    private ServiceProvider? _services;
 
    [Fact]
    public async Task ProcessInputAsync_WelcomesAndPromptsAuth_AndSavesToDb_ForStartCommandOfUnauthenticatedUser()
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
        var (services, container) = serviceCollection.ConfigureTestRepositories();
        var inputProcessor = services.GetRequiredService<IInputProcessor>();

        var glossary = services.GetRequiredService<IDomainGlossary>();
        var expectedTlgInputSavedToDb = 
            startCommand with
            {
                ResultantWorkflow = new ResultantWorkflowState(
                    glossary.GetId(typeof(IUserAuthWorkflow)),
                    glossary.GetId(UserAuthWorkflow.States.Initial))
            };
        
        var mockInputRepo = (Mock<ITlgInputsRepository>)container.Mocks[typeof(ITlgInputsRepository)];
        mockInputRepo
            .Setup(repo => 
                repo.AddAsync(It.Is<TlgInput>(input => 
                    input.Equals(expectedTlgInputSavedToDb))))
            .Verifiable();
        
        List<OutputDto> expectedOutputs = [
            new(){ Text = Ui("🫡 Welcome to the CheckMade ChatBot. I shall follow your command!") },
            new(){ Text = UserAuthWorkflow.EnterTokenPrompt.Text.GetValueOrThrow() }];
        
        var actualOutput = 
            await inputProcessor
                .ProcessInputAsync(startCommand);

        Assert.Equivalent(
            expectedOutputs,
            actualOutput);
        
        mockInputRepo.Verify();
    }
    
    [Fact]
    public async Task ProcessInputAsync_ReturnsWarning_AndSavesToDb_ForOutOfScopeCallbackQuery()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        // Back to messageId: 4 because user clicked on button in chat history, but /settings now out-of-scope
        var outOfScopeCallbackQuery =
            inputGenerator.GetValidTlgInputCallbackQueryForDomainTerm(Dt(LanguageCode.en), 
                messageId: 4);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
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
                    messageId: 6)
            });
        var inputProcessor = services.GetRequiredService<IInputProcessor>();

        var expectedTlgInputSavedToDbWithoutResultantWorkflowState = outOfScopeCallbackQuery;
        
        var mockInputRepo = (Mock<ITlgInputsRepository>)container.Mocks[typeof(ITlgInputsRepository)];
        mockInputRepo
            .Setup(repo => 
                repo.AddAsync(It.Is<TlgInput>(input => 
                    input.Equals(expectedTlgInputSavedToDbWithoutResultantWorkflowState))))
            .Verifiable();
        
        const string expectedWarningOutput = 
            "The previous workflow was completed, so your last message/action will be ignored.";
        
        var actualOutput = 
            await inputProcessor
                .ProcessInputAsync(outOfScopeCallbackQuery);
        
        Assert.Equal(
            expectedWarningOutput,
            TestUtils.GetFirstRawEnglish(actualOutput));
        
        mockInputRepo.Verify();
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
                    (int)OperationsBotCommands.Settings)
            });
        var inputProcessor = services.GetRequiredService<IInputProcessor>();
        
        const string expectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        
        var actualOutput =
            await inputProcessor
                .ProcessInputAsync(interruptingBotCommandInput);
        
        Assert.Equal(
            expectedWarningOutput,
            TestUtils.GetFirstRawEnglish(actualOutput));
    }
    
    [Fact]
    public async Task ProcessInputAsync_NoWarning_ForNewBotCommand_WhenUserCompletedPreviousWorkflow()
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
                    Dt(LanguageCode.de))});
        var inputProcessor = services.GetRequiredService<IInputProcessor>();
        
        const string notExpectedWarningOutput = 
            "FYI: you interrupted the previous workflow before its completion or successful submission.";
        
        var actualOutput = 
            await inputProcessor
                .ProcessInputAsync(notInterruptingBotCommandInput);
        
        Assert.NotEqual(
            notExpectedWarningOutput,
            TestUtils.GetFirstRawEnglish(actualOutput));
    }

    [Fact]
    public async Task ProcessInputAsync_ReturnsEmptyOutput_AndSavesToDb_ForLocationUpdate()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var locationUpdate =
            inputGenerator.GetValidTlgInputLocationMessage(
                new Geo(17, -22, Option<double>.None()));
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories();
        var inputProcessor = services.GetRequiredService<IInputProcessor>();

        var expectedTlgInputSavedToDbWithoutResultantWorkflowState = locationUpdate;

        var mockInputRepo = (Mock<ITlgInputsRepository>)container.Mocks[typeof(ITlgInputsRepository)];
        mockInputRepo
            .Setup(repo => 
                repo.AddAsync(It.Is<TlgInput>(input => 
                    input.Equals(expectedTlgInputSavedToDbWithoutResultantWorkflowState))))
            .Verifiable();
        
        var actualOutput =
            await inputProcessor
                .ProcessInputAsync(locationUpdate);
        
        Assert.Empty(actualOutput);
        mockInputRepo.Verify();
    }
}