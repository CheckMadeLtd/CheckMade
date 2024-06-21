using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
using static CheckMade.Tests.ITestUtils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.UserAuthWorkflow.States;
using InputValidator = CheckMade.Common.LangExt.InputValidator;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic.Workflows;

public class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsReceivedTokenSubmissionAttempt_AfterUserEnteredAnyText()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestUserId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { utils.GetValidTlgInputTextMessage() });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(tlgAgent);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }

    [Fact]
    public async Task DetermineCurrentStateAsync_OnlyConsidersInputs_SinceDeactivationOfLastTlgAgentRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_02, TestChatId_03, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    
        // Depends on an 'expired' tlgAgentRole set up by default in the MockTlgAgentRoleRepository 
        var tlgPastInputToBeIgnored = utils.GetValidTlgInputTextMessage(
            tlgAgent.UserId,
            tlgAgent.ChatId,
            SanitaryOpsAdmin1AtMockParooka2024.Token,
            new DateTime(1999, 01, 05));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { tlgPastInputToBeIgnored });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(tlgAgent);
        
        Assert.Equal(Initial, actualState);
    }

    [Fact]
    // We know it's failed because a successful submission means no revisiting of the UserAuthWorkflow!
    public async Task DetermineCurrentStateAsync_ReturnsReceivedTokenSubmissionAttempt_AfterFailedAttempt()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var utils = _services.GetRequiredService<ITestUtils>();
        
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { utils.GetValidTlgInputTextMessage(text: "InvalidToken") });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(tlgAgent);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ReturnsCorrectErrorMessage_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        var nonExistingTokenInput = utils.GetValidTlgInputTextMessage(
            text: InputValidator.GetTokenFormatExample(),
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { nonExistingTokenInput });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
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
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputTokenWithPreExistingActiveTlgAgentRole = utils.GetValidTlgInputTextMessage(
            text: SanitaryOpsAdmin1AtMockParooka2024.Token,
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputTokenWithPreExistingActiveTlgAgentRole });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRolesRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        var preExistingActiveTlgAgentRole = (await mockTlgAgentRolesRepo.Object.GetAllAsync())
            .First(arb => 
                arb.Role.Token == SanitaryOpsAdmin1AtMockParooka2024.Token &&
                arb.TlgAgent.Mode == tlgAgent.Mode);
        
        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another {0} chat. 
                                       This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                                       """;

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualOutputs = 
            await workflow.GetNextOutputAsync(inputTokenWithPreExistingActiveTlgAgentRole);
        
        Assert.Equal(expectedWarning, GetFirstRawEnglish(actualOutputs));
        
        mockTlgAgentRolesRepo.Verify(x => 
            x.UpdateStatusAsync(preExistingActiveTlgAgentRole, DbRecordStatus.Historic));
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesTlgAgentRole_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_03, TestChatId_08, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputValidToken = utils.GetValidTlgInputTextMessage(
            // Diverging userId and chatId = sent from a Tlgr Chat-Group (rather than a private chat with the bot)
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: SanitaryOpsInspector2AtMockHurricane2024.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRolesRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        const string expectedConfirmation = """
                                            {0}, welcome to the CheckMade ChatBot!
                                            You have successfully authenticated as a {1} at live-event {2}.
                                            """;
        
        var expectedTlgAgentRoleAdded = new TlgAgentRoleBind(
            SanitaryOpsInspector2AtMockHurricane2024,
            tlgAgent,
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var actualTlgAgentRoleAdded = new List<TlgAgentRoleBind>(); 
        mockTlgAgentRolesRepo
            .Setup(x => 
                x.AddAsync(It.IsAny<IEnumerable<TlgAgentRoleBind>>()))
            .Callback<IEnumerable<TlgAgentRoleBind>>(tlgAgentRole => 
                actualTlgAgentRoleAdded = tlgAgentRole.ToList());
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        var actualOutputs = await workflow.GetNextOutputAsync(inputValidToken);
        
        Assert.Equal(expectedConfirmation, GetFirstRawEnglish(actualOutputs));
        Assert.Equivalent(expectedTlgAgentRoleAdded.Role, actualTlgAgentRoleAdded[0].Role);
        Assert.Equivalent(expectedTlgAgentRoleAdded.TlgAgent, actualTlgAgentRoleAdded[0].TlgAgent);
        Assert.Equivalent(expectedTlgAgentRoleAdded.Status, actualTlgAgentRoleAdded[0].Status);
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesTlgAgentRolesForAllModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        const long privateChatUserAndChatId = TestUserId_03;
        var tlgAgent = new TlgAgent(privateChatUserAndChatId, privateChatUserAndChatId, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputValidToken = utils.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            text: SanitaryOpsInspector2AtMockHurricane2024.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRolesRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
        
        var expectedTlgAgentRolesAdded = allModes.Select(im => 
            new TlgAgentRoleBind(
                SanitaryOpsInspector2AtMockHurricane2024,
                tlgAgent with { Mode = im },
                DateTime.UtcNow,
                Option<DateTime>.None()))
            .ToImmutableReadOnlyList();

        var actualTlgAgentRoles = new List<TlgAgentRoleBind>();
        mockTlgAgentRolesRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IEnumerable<TlgAgentRoleBind>>()))
            .Callback<IEnumerable<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoles = tlgAgentRoles.ToList());

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        await workflow.GetNextOutputAsync(inputValidToken);
        
        mockTlgAgentRolesRepo.Verify(x => x.AddAsync(
            It.IsAny<IEnumerable<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRolesAdded.Count; i++)
        {
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].TlgAgent, actualTlgAgentRoles[i].TlgAgent);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Role, actualTlgAgentRoles[i].Role);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Status, actualTlgAgentRoles[i].Status);
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
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputValidToken = utils.GetValidTlgInputTextMessage(
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId,
            // Already has a mapped TlgAgentRole for 'Communications' (see UnitTestStartup)
            text: SanitaryOpsEngineer2.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockTlgAgentRolesRepo = _services.GetRequiredService<Mock<ITlgAgentRoleBindingsRepository>>();

        var expectedTlgAgentRolesAdded = new List<TlgAgentRoleBind>
        {
            new(SanitaryOpsEngineer2,
                tlgAgent,
                DateTime.UtcNow,
                Option<DateTime>.None()),
            
            new(SanitaryOpsEngineer2,
                tlgAgent with { Mode = Notifications },
                DateTime.UtcNow,
                Option<DateTime>.None()),
        };

        var actualTlgAgentRoles = new List<TlgAgentRoleBind>();
        mockTlgAgentRolesRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IEnumerable<TlgAgentRoleBind>>()))
            .Callback<IEnumerable<TlgAgentRoleBind>>(
                tlgAgentRoles => actualTlgAgentRoles = tlgAgentRoles.ToList());
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        await workflow.GetNextOutputAsync(inputValidToken);
        
        mockTlgAgentRolesRepo.Verify(x => x.AddAsync(
            It.IsAny<IEnumerable<TlgAgentRoleBind>>()));

        for (var i = 0; i < expectedTlgAgentRolesAdded.Count; i++)
        {
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].TlgAgent, actualTlgAgentRoles[i].TlgAgent);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Role, actualTlgAgentRoles[i].Role);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Status, actualTlgAgentRoles[i].Status);
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
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        var badTokenInput = utils.GetValidTlgInputTextMessage(text: badToken);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent))
            .ReturnsAsync(new List<TlgInput> { badTokenInput });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.Equal("Bad token format! Try again...", GetFirstRawEnglish(actualOutputs));
    }
}