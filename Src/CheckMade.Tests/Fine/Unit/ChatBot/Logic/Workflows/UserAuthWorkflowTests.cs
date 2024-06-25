using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.UserAuthWorkflow.States;
using InputValidator = CheckMade.Common.LangExt.InputValidator;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

public class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public void DetermineCurrentState_ReturnsReceivedTokenSubmissionAttempt_AfterUserEnteredAnyText()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestUserId_01_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput> { utils.GetValidTlgInputTextMessage() }; 
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }

    [Fact]
    public async Task DetermineCurrentState_OnlyConsidersInputs_SinceDeactivationOfLastTlgAgentRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_02, TestChatId_03, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
    
        // Depends on an 'expired' tlgAgentRole set up by default in the MockTlgAgentRoleRepository 
        var tlgPastInputToBeIgnored = utils.GetValidTlgInputTextMessage(
            tlgAgent.UserId,
            tlgAgent.ChatId,
            SanitaryOpsEngineer1.Token,
            new DateTime(1999, 01, 05));

        var inputHistory = new List<TlgInput> { tlgPastInputToBeIgnored };
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        var logicUtils = _services.GetRequiredService<ILogicUtils>();
        var relevantHistory = 
            await logicUtils.GetAllInputsOfTlgAgentInCurrentRoleAsync(tlgAgent);
        
        var actualState = workflow.DetermineCurrentState(relevantHistory);
        
        Assert.Equal(Initial, actualState);
    }

    [Fact]
    // We know it's failed because a successful submission means no revisiting of the UserAuthWorkflow!
    public void DetermineCurrentState_ReturnsReceivedTokenSubmissionAttempt_AfterFailedAttempt()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
        var utils = _services.GetRequiredService<ITestUtils>();
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputHistory = new List<TlgInput> { utils.GetValidTlgInputTextMessage(text: "InvalidToken") };
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(inputHistory);
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = workflow.DetermineCurrentState(inputHistory);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ReturnsCorrectErrorMessage_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        var nonExistingTokenInput = utils.GetValidTlgInputTextMessage(
            text: InputValidator.GetTokenFormatExample(),
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { nonExistingTokenInput });
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = await workflow.GetNextOutputAsync(nonExistingTokenInput);
        
        Assert.Equal("This is an unknown token. Try again...", GetFirstRawEnglish(actualOutputs));
    }

    [Fact]
    public async Task GetNextOutputAsync_ReturnsWarning_AndDeactivatesPreExisting_WhenTokenAlreadyHasActiveTlgAgentRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputTokenWithPreExistingActiveTlgAgentRoleBind = utils.GetValidTlgInputTextMessage(
            text: DanielIsSanitaryOpsAdminAtMockParooka2024Default.Token,
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputTokenWithPreExistingActiveTlgAgentRoleBind });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRoleBindingsRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        var preExistingActiveTlgAgentRoleBind = (await mockTlgAgentRoleBindingsRepo.Object.GetAllAsync())
            .First(arb => 
                arb.Role.Token == DanielIsSanitaryOpsAdminAtMockParooka2024Default.Token &&
                arb.TlgAgent.Mode == tlgAgent.Mode);
        
        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another {0} chat. 
                                       This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                                       """;

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualOutputs = 
            await workflow.GetNextOutputAsync(inputTokenWithPreExistingActiveTlgAgentRoleBind);
        
        Assert.Equal(expectedWarning, GetFirstRawEnglish(actualOutputs));
        
        mockTlgAgentRoleBindingsRepo.Verify(x => 
            x.UpdateStatusAsync(preExistingActiveTlgAgentRoleBind, DbRecordStatus.Historic));
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesTlgAgentRole_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_03, TestChatId_08, Operations);
        var roleForAuth = DanielIsSanitaryOpsInspectorAtMockHurricane2024;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputValidToken = utils.GetValidTlgInputTextMessage(
            // Diverging userId and chatId = sent from a Tlgr Chat-Group (rather than a private chat with the bot)
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRolesRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        const string expectedConfirmation = """
                                            {0}, welcome to the CheckMade ChatBot!
                                            You have successfully authenticated as a {1} at live-event {2}.
                                            """;
        
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
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        var actualOutputs = await workflow.GetNextOutputAsync(inputValidToken);
        
        Assert.Equal(expectedConfirmation, GetFirstRawEnglish(actualOutputs));
        Assert.Equivalent(expectedTlgAgentRoleBindAdded.Role, actualTlgAgentRoleBindAdded[0].Role);
        Assert.Equivalent(expectedTlgAgentRoleBindAdded.TlgAgent, actualTlgAgentRoleBindAdded[0].TlgAgent);
        Assert.Equivalent(expectedTlgAgentRoleBindAdded.Status, actualTlgAgentRoleBindAdded[0].Status);
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesTlgAgentRolesForAllModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        const long privateChatUserAndChatId = TestUserId_03;
        var tlgAgent = new TlgAgent(privateChatUserAndChatId, privateChatUserAndChatId, Operations);
        var roleForAuth = DanielIsSanitaryOpsInspectorAtMockHurricane2024;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputValidToken = utils.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: roleForAuth.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRoleBindingsRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

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

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        await workflow.GetNextOutputAsync(inputValidToken);
        
        mockTlgAgentRoleBindingsRepo.Verify(x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(expectedTlgAgentRoleBindingsAdded[i].TlgAgent, actualTlgAgentRoleBindingsAdded[i].TlgAgent);
            Assert.Equivalent(expectedTlgAgentRoleBindingsAdded[i].Role, actualTlgAgentRoleBindingsAdded[i].Role);
            Assert.Equivalent(expectedTlgAgentRoleBindingsAdded[i].Status, actualTlgAgentRoleBindingsAdded[i].Status);
        }
    }
    
    [Fact]
    public async Task GetNextOutputAsync_CreatesTlgAgentRolesForMissingMode_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        const long privateChatUserAndChatId = TestUserId_03;
        var tlgAgent = new TlgAgent(privateChatUserAndChatId, privateChatUserAndChatId, Operations);
        var roleForAuth = SanitaryOpsEngineer2;
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();

        var inputValidToken = utils.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            // Already has a mapped TlgAgentRole for 'Communications' (see UnitTestStartup)
            text: roleForAuth.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRoleBindingsRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        var expectedTlgAgentRoleBindingsAdded = new List<TlgAgentRoleBind>
        {
            new(roleForAuth,
                tlgAgent,
                DateTime.UtcNow,
                Option<DateTime>.None()),
            
            new(roleForAuth,
                tlgAgent with { Mode = Notifications },
                DateTime.UtcNow,
                Option<DateTime>.None()),
        };

        var actualTlgAgentRoleBindings = new List<TlgAgentRoleBind>();
        mockTlgAgentRoleBindingsRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()))
            .Callback<IReadOnlyCollection<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoleBindings = tlgAgentRoles.ToList());
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        await workflow.GetNextOutputAsync(inputValidToken);
        
        mockTlgAgentRoleBindingsRepo.Verify(x => x.AddAsync(
            It.IsAny<IReadOnlyCollection<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRoleBindingsAdded.Count; i++)
        {
            Assert.Equivalent(expectedTlgAgentRoleBindingsAdded[i].TlgAgent, actualTlgAgentRoleBindings[i].TlgAgent);
            Assert.Equivalent(expectedTlgAgentRoleBindingsAdded[i].Role, actualTlgAgentRoleBindings[i].Role);
            Assert.Equivalent(expectedTlgAgentRoleBindingsAdded[i].Status, actualTlgAgentRoleBindings[i].Status);
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
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01_Default, TestChatId_01_Default, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        var badTokenInput = utils.GetValidTlgInputTextMessage(text: badToken);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { badTokenInput });
        
        serviceCollection.AddScoped<ITlgInputsRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.Equal("Bad token format! Try again...", GetFirstRawEnglish(actualOutputs));
    }
}