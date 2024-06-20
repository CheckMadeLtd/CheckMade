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
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .ReturnsAsync(new List<TlgInput> { utils.GetValidTlgInputTextMessage() });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualState = await workflow.DetermineCurrentStateAsync(tlgAgent);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }

    [Fact]
    public async Task DetermineCurrentStateAsync_OnlyConsidersInputs_SinceDeactivationOfLastTlgTlgAgentRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_02, TestChatId_03, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    
        // Depends on an 'expired' tlgAgentRole set up by default in the MockTlgTlgAgentRoleRepository 
        var tlgPastInputToBeIgnored = utils.GetValidTlgInputTextMessage(
            tlgAgent.UserId,
            tlgAgent.ChatId,
            SanitaryOpsAdmin1.Token,
            new DateTime(1999, 01, 05));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
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
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
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
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .ReturnsAsync(new List<TlgInput> { nonExistingTokenInput });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = await workflow.GetNextOutputAsync(nonExistingTokenInput);
        
        Assert.Equal("This is an unknown token. Try again...", GetFirstRawEnglish(actualOutputs));
    }

    [Fact]
    public async Task GetNextOutputAsync_ReturnsWarning_AndDeactivatesPreExisting_WhenTokenAlreadyHasActivePortRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var serviceCollection = new UnitTestStartup().Services;
        
        var utils = _services.GetRequiredService<ITestUtils>();
        var tlgAgent = new TlgAgent(TestUserId_01, TestChatId_01, Operations);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputTokenWithPreExistingActivePortRole = utils.GetValidTlgInputTextMessage(
            text: SanitaryOpsAdmin1.Token,
            userId: tlgAgent.UserId,
            chatId: tlgAgent.ChatId);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .ReturnsAsync(new List<TlgInput> { inputTokenWithPreExistingActivePortRole });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockPortRolesRepo = _services.GetRequiredService<Mock<ITlgTlgAgentRoleRepository>>();

        var preExistingActivePortRole = (await mockPortRolesRepo.Object.GetAllAsync())
            .First(cpr => 
                cpr.Role.Token == SanitaryOpsAdmin1.Token &&
                cpr.TlgAgent.Mode == tlgAgent.Mode);
        
        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another {0} chat. 
                                       This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                                       """;

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
        
        var actualOutputs = 
            await workflow.GetNextOutputAsync(inputTokenWithPreExistingActivePortRole);
        
        Assert.Equal(expectedWarning, GetFirstRawEnglish(actualOutputs));
        
        mockPortRolesRepo.Verify(x => 
            x.UpdateStatusAsync(preExistingActivePortRole, DbRecordStatus.Historic));
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesPortRole_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup()
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
            text: SanitaryOpsInspector2.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockPortRolesRepo = _services.GetRequiredService<Mock<ITlgTlgAgentRoleRepository>>();

        const string expectedConfirmation = """
                                            {0}, welcome to the CheckMade ChatBot!
                                            You have successfully authenticated as a {1} at live-event {2}.
                                            """;
        
        var expectedTlgAgentRoleAdded = new TlgTlgAgentRole(
            SanitaryOpsInspector2,
            tlgAgent,
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var actualTlgAgentRoleAdded = new List<TlgTlgAgentRole>(); 
        mockPortRolesRepo
            .Setup(x => 
                x.AddAsync(It.IsAny<IEnumerable<TlgTlgAgentRole>>()))
            .Callback<IEnumerable<TlgTlgAgentRole>>(portRole => 
                actualTlgAgentRoleAdded = portRole.ToList());
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        var actualOutputs = await workflow.GetNextOutputAsync(inputValidToken);
        
        Assert.Equal(expectedConfirmation, GetFirstRawEnglish(actualOutputs));
        Assert.Equivalent(expectedTlgAgentRoleAdded.Role, actualTlgAgentRoleAdded[0].Role);
        Assert.Equivalent(expectedTlgAgentRoleAdded.TlgAgent, actualTlgAgentRoleAdded[0].TlgAgent);
        Assert.Equivalent(expectedTlgAgentRoleAdded.Status, actualTlgAgentRoleAdded[0].Status);
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesPortRolesForAllModes_WhenValidTokenSubmitted_FromPrivateChat()
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
            text: SanitaryOpsInspector2.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockPortRolesRepo = _services.GetRequiredService<Mock<ITlgTlgAgentRoleRepository>>();

        var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
        
        var expectedTlgAgentRolesAdded = allModes.Select(im => 
            new TlgTlgAgentRole(
                SanitaryOpsInspector2,
                tlgAgent with { Mode = im },
                DateTime.UtcNow,
                Option<DateTime>.None()))
            .ToList();

        var actualPortRoles = new List<TlgTlgAgentRole>();
        mockPortRolesRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IEnumerable<TlgTlgAgentRole>>()))
            .Callback<IEnumerable<TlgTlgAgentRole>>(
                portRoles => actualPortRoles = portRoles.ToList());

        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        await workflow.GetNextOutputAsync(inputValidToken);
        
        mockPortRolesRepo.Verify(x => x.AddAsync(
            It.IsAny<IEnumerable<TlgTlgAgentRole>>()));

        for (var i = 0; i < expectedTlgAgentRolesAdded.Count; i++)
        {
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].TlgAgent, actualPortRoles[i].TlgAgent);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Role, actualPortRoles[i].Role);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Status, actualPortRoles[i].Status);
        }
    }
    
    [Fact]
    public async Task GetNextOutputAsync_CreatesPortRolesForMissingMode_WhenValidTokenSubmitted_FromPrivateChat()
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
            // Already has a mapped PortRole for 'Communications' (see UnitTestStartup)
            text: SanitaryOpsEngineer2.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var mockPortRolesRepo = _services.GetRequiredService<Mock<ITlgTlgAgentRoleRepository>>();

        var expectedTlgAgentRolesAdded = new List<TlgTlgAgentRole>
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

        var actualPortRoles = new List<TlgTlgAgentRole>();
        mockPortRolesRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IEnumerable<TlgTlgAgentRole>>()))
            .Callback<IEnumerable<TlgTlgAgentRole>>(
                portRoles => actualPortRoles = portRoles.ToList());
        
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();

        await workflow.GetNextOutputAsync(inputValidToken);
        
        mockPortRolesRepo.Verify(x => x.AddAsync(
            It.IsAny<IEnumerable<TlgTlgAgentRole>>()));

        for (var i = 0; i < expectedTlgAgentRolesAdded.Count; i++)
        {
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].TlgAgent, actualPortRoles[i].TlgAgent);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Role, actualPortRoles[i].Role);
            Assert.Equivalent(expectedTlgAgentRolesAdded[i].Status, actualPortRoles[i].Status);
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
            .Setup(repo => repo.GetAllAsync(tlgAgent.UserId, tlgAgent.ChatId))
            .ReturnsAsync(new List<TlgInput> { badTokenInput });
        
        serviceCollection.AddScoped<ITlgInputRepository>(_ => mockTlgInputsRepo.Object);
        _services = serviceCollection.BuildServiceProvider();
        var workflow = _services.GetRequiredService<IUserAuthWorkflow>();
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.Equal("Bad token format! Try again...", GetFirstRawEnglish(actualOutputs));
    }
}