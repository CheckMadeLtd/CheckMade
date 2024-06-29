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
// ReSharper disable MoveLocalFunctionAfterJumpStatement

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

public class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public void DetermineCurrentState_ReturnsReceivedTokenSubmissionAttempt_AfterUserEnteredAnyText()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput> { inputGenerator.GetValidTlgInputTextMessage() }; 
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(
            ReceivedTokenSubmissionAttempt,
            actualState);
    }

    [Fact]
    public async Task DetermineCurrentState_OnlyConsidersInputs_SinceDeactivationOfLastTlgAgentRole()
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

        var inputHistory = 
            new List<TlgInput> { tlgPastInputToBeIgnored }
                .ToImmutableReadOnlyCollection();

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.SetupMockRepositories(
            roleBindings: new []{ historicRoleBind }.ToImmutableReadOnlyCollection(),
            inputs: inputHistory);
        _services = services;
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        var logicUtils = _services.GetRequiredService<ILogicUtils>();
        var tlgAgentInputHistory = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent);
        
        var actualState = workflow.DetermineCurrentState(tlgAgentInputHistory);
        
        Assert.Equal(
            Initial, // Instead of 'ReceivedTokenSubmissionAttempt'
            actualState);
    }

    [Fact]
    // We know it's failed because a successful submission means no revisiting of the UserAuthWorkflow!
    public void DetermineCurrentState_ReturnsReceivedTokenSubmissionAttempt_AfterFailedAttempt()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var tlgAgent = PrivateBotChat_Operations;
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput> { inputGenerator.GetValidTlgInputTextMessage(text: "InvalidToken") };
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(
            ReceivedTokenSubmissionAttempt,
            actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ReturnsCorrectErrorMessage_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        var nonExistingTokenInput = inputGenerator.GetValidTlgInputTextMessage(
            text: InputValidator.GetTokenFormatExample());
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { nonExistingTokenInput });
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
    
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
        var tlgAgent = PrivateBotChat_Operations;
        
        var inputTokenWithPreExistingActiveTlgAgentRoleBind = 
            inputGenerator.GetValidTlgInputTextMessage(
                text: SOpsAdmin_DanielEn_X2024.Token);
        
        var preExistingActiveTlgAgentRoleBind = 
            MockRepositoryUtils.GetNewRoleBind(
                SOpsAdmin_DanielEn_X2024, tlgAgent);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, container) = serviceCollection.SetupMockRepositories(
            roleBindings: new[] { preExistingActiveTlgAgentRoleBind },
            inputs: new[] { inputTokenWithPreExistingActiveTlgAgentRoleBind });
        _services = services;

        var mockTlgAgentRoleBindingsRepo = 
            (Mock<ITlgAgentRoleBindingsRepository>)container.Mocks[typeof(ITlgAgentRoleBindingsRepository)];
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
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
        var serviceCollection = new UnitTestStartup().Services;
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = UserId03_ChatId06_Operations;
        var roleForAuth = SOpsEngineer_DanielEn_X2024;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRolesRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        const string expectedConfirmation = "{0}, you have successfully authenticated as a {1} at live-event {2}.";
        
        var expectedTlgAgentRoleBindAdded = new TlgAgentRoleBind(
            roleForAuth,
            tlgAgent,
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var actualTlgAgentRoleBindAdded = new List<TlgAgentRoleBind>(); 
        mockTlgAgentRolesRepo
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
        var serviceCollection = new UnitTestStartup().Services;
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var roleForAuth = SOpsInspector_DanielDe_X2024;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRoleBindingsRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
        
        var expectedTlgAgentRoleBindingsAdded = allModes.Select(im => 
            new TlgAgentRoleBind(
                roleForAuth,
                tlgAgent with { Mode = im },
                DateTime.UtcNow,
                Option<DateTime>.None()))
            .ToImmutableReadOnlyList();

        var actualTlgAgentRoleBindingsAdded = new List<TlgAgentRoleBind>();
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
        var serviceCollection = new UnitTestStartup().Services;
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var roleForAuth = SOpsEngineer_DanielEn_X2024;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputValidToken = inputGenerator.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRoleBindingsRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        var expectedTlgAgentRoleBindingsAdded = new List<TlgAgentRoleBind>
        {
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
        };

        var actualTlgAgentRoleBindingsAdded = new List<TlgAgentRoleBind>();
        mockTlgAgentRoleBindingsRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoleBindingsAdded = tlgAgentRoles.ToList());
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

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
        var serviceCollection = new UnitTestStartup().Services;
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        var badTokenInput = inputGenerator.GetValidTlgInputTextMessage(text: badToken);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { badTokenInput });
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = await workflow.GetResponseAsync(badTokenInput);
        
        Assert.Equal("Bad token format! Try again...", TestUtils.GetFirstRawEnglish(actualOutputs));
    }
}