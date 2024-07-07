using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.UserAuthWorkflow.States;
using InputValidator = CheckMade.Common.LangExt.InputValidator;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public void DetermineCurrentState_ReturnsReceivedTokenSubmissionAttempt_AfterUserEnteredAnyText()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        List<TlgInput> inputHistory = [inputGenerator.GetValidTlgInputTextMessage()];
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(
            ReceivedTokenSubmissionAttempt,
            actualState);
    }

    [Fact]
    public async Task DetermineCurrentState_OnlyConsidersInputs_SinceDeactivationOfLastRoleBind()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = UserId02_ChatId03_Operations;
        
        var historicRoleBind = new TlgAgentRoleBind(
            SOpsAdmin_DanielEn_X2024,
            tlgAgent,
            new DateTime(2001, 09, 27),
            new DateTime(2001, 09, 30),
            DbRecordStatus.Historic);

        var tlgPastInputToBeIgnored = 
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId,
                tlgAgent.ChatId,
                historicRoleBind.Role.Token,
                historicRoleBind
                    .DeactivationDate.GetValueOrThrow()
                    .AddDays(-1));

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: new []{ historicRoleBind },
            inputs: new []{ tlgPastInputToBeIgnored });
        var workflow = services.GetRequiredService<IUserAuthWorkflow>();
        var logicUtils = services.GetRequiredService<ILogicUtils>();
        
        var tlgAgentInputHistory = 
            await logicUtils.GetAllCurrentInteractiveAsync(tlgAgent);
        
        var actualState = workflow.DetermineCurrentState(tlgAgentInputHistory);
        
        Assert.Equal(
            Initial, // Instead of 'ReceivedTokenSubmissionAttempt'
            actualState);
    }

    [Fact]
    public async Task GetNextOutputAsync_ReturnsCorrectErrorMessage_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var nonExistingTokenInput = inputGenerator.GetValidTlgInputTextMessage(
            text: InputValidator.GetTokenFormatExample());
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: new[] { nonExistingTokenInput });
        var workflow = services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = 
            await workflow
                .GetResponseAsync(nonExistingTokenInput);
        
        Assert.Equal(
            "This is an unknown token. Try again...",
            TestUtils.GetFirstRawEnglish(actualOutputs));
    }

    [Fact]
    public async Task GetNextOutputAsync_ReturnsWarning_AndDeactivatesPreExisting_WhenTokenAlreadyHasActiveTlgAgentRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var inputTokenWithPreExistingActiveTlgAgentRoleBind = 
            inputGenerator.GetValidTlgInputTextMessage(
                text: SOpsAdmin_DanielEn_X2024.Token);
        
        var preExistingActiveTlgAgentRoleBind = 
            TestRepositoryUtils.GetNewRoleBind(
                SOpsAdmin_DanielEn_X2024,
                PrivateBotChat_Operations);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            roleBindings: new[] { preExistingActiveTlgAgentRoleBind },
            inputs: new[] { inputTokenWithPreExistingActiveTlgAgentRoleBind });
        var mockTlgAgentRoleBindingsRepo = 
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<IUserAuthWorkflow>();
        
        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another {0} chat. 
                                       This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                                       """;
        
        var actualOutputs = 
            await workflow
                .GetResponseAsync(inputTokenWithPreExistingActiveTlgAgentRoleBind);
        
        Assert.Equal(
            expectedWarning,
            TestUtils.GetFirstRawEnglish(actualOutputs));
        
        mockTlgAgentRoleBindingsRepo.Verify(x => 
            x.UpdateStatusAsync(
                preExistingActiveTlgAgentRoleBind,
                DbRecordStatus.Historic));
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesRoleBind_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = UserId03_ChatId06_Operations;
        var roleForAuth = SOpsEngineer_DanielEn_X2024;

        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            roles: new []{ SOpsEngineer_DanielEn_X2024 },
            inputs: new[] { inputValidToken });
        var mockRoleBindingsRepo =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<IUserAuthWorkflow>();
        
        const string expectedConfirmation = "{0}, you have successfully authenticated as a {1} at live-event {2}.";
        
        var expectedTlgAgentRoleBindAdded = new TlgAgentRoleBind(
            roleForAuth,
            tlgAgent,
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        List<TlgAgentRoleBind> actualTlgAgentRoleBindAdded = []; 
        mockRoleBindingsRepo
            .Setup(x => 
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(tlgAgentRole => 
                actualTlgAgentRoleBindAdded = tlgAgentRole.ToList());
        
        var actualOutputs = await workflow.GetResponseAsync(inputValidToken);
        
        Assert.Equal(
            expectedConfirmation,
            TestUtils.GetFirstRawEnglish(actualOutputs));
        Assert.Equivalent(
            expectedTlgAgentRoleBindAdded.Role,
            actualTlgAgentRoleBindAdded[0].Role);
        Assert.Equivalent(
            expectedTlgAgentRoleBindAdded.TlgAgent,
            actualTlgAgentRoleBindAdded[0].TlgAgent);
        Assert.Equivalent(
            expectedTlgAgentRoleBindAdded.Status,
            actualTlgAgentRoleBindAdded[0].Status);
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesRoleBindingsForAllModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var roleForAuth = SOpsInspector_DanielDe_X2024;
        
        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            roles: new []{ SOpsInspector_DanielDe_X2024 },
            inputs: new[] { inputValidToken });
        var mockTlgAgentRoleBindingsRepo =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = services.GetRequiredService<IUserAuthWorkflow>();
        
        var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
        var expectedTlgAgentRoleBindingsAdded = allModes.Select(im => 
            new TlgAgentRoleBind(
                roleForAuth,
                tlgAgent with { Mode = im },
                DateTime.UtcNow,
                Option<DateTime>.None()))
            .ToImmutableReadOnlyList();

        List<TlgAgentRoleBind> actualTlgAgentRoleBindingsAdded = [];
        mockTlgAgentRoleBindingsRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoleBindingsAdded = tlgAgentRoles.ToList());

        await workflow.GetResponseAsync(inputValidToken);
        
        mockTlgAgentRoleBindingsRepo.Verify(x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].TlgAgent,
                actualTlgAgentRoleBindingsAdded[i].TlgAgent);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Role,
                actualTlgAgentRoleBindingsAdded[i].Role);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Status,
                actualTlgAgentRoleBindingsAdded[i].Status);
        }
    }
    
    [Fact]
    public async Task GetNextOutputAsync_CreatesRoleBindingsForMissingModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var roleForAuth = SOpsEngineer_DanielEn_X2024;

        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.ConfigureTestRepositories(
            roles: new[] { SOpsEngineer_DanielEn_X2024 },
            roleBindings: new []{ TestRepositoryUtils.GetNewRoleBind(
                SOpsEngineer_DanielEn_X2024, tlgAgent with { Mode = Communications }) },
            inputs: new[] { inputValidToken });
        var mockTlgAgentRoleBindingsRepo =
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];

        List<TlgAgentRoleBind> expectedTlgAgentRoleBindingsAdded = [ 
        
            // Adds missing bind for Operations Mode
            new(roleForAuth,
                tlgAgent,
                DateTime.UtcNow,
                Option<DateTime>.None()),

            // Adds missing bind for Notifications Mode
            new(roleForAuth,
                tlgAgent with { Mode = Notifications },
                DateTime.UtcNow,
                Option<DateTime>.None()),
        ];

        List<TlgAgentRoleBind> actualTlgAgentRoleBindingsAdded = [];
        mockTlgAgentRoleBindingsRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoleBindingsAdded = tlgAgentRoles.ToList());
        
        var workflow = services.GetRequiredService<IUserAuthWorkflow>();

        await workflow.GetResponseAsync(inputValidToken);
        
        mockTlgAgentRoleBindingsRepo.Verify(x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].TlgAgent,
                actualTlgAgentRoleBindingsAdded[i].TlgAgent);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Role, 
                actualTlgAgentRoleBindingsAdded[i].Role);
            Assert.Equivalent(
                expectedTlgAgentRoleBindingsAdded[i].Status,
                actualTlgAgentRoleBindingsAdded[i].Status);
        }
    }
    
    [Theory]
    [InlineData("5JFUX")]
    [InlineData(" ")]
    [InlineData(" some text with trailing spaces and \n line break ")]
    [InlineData("")]
    public async Task GetNextOutputAsync_ReturnsCorrectErrorMessage_WhenBadFormatTokenEntered(string badToken)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var badTokenInput = inputGenerator.GetValidTlgInputTextMessage(text: badToken);
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: new[] { badTokenInput });
        var workflow = services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = await workflow.GetResponseAsync(badTokenInput);
        
        Assert.Equal("Bad token format! Try again...", TestUtils.GetFirstRawEnglish(actualOutputs));
    }
}