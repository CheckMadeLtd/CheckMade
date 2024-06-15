using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Utils.Generic;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static CheckMade.Tests.ITestUtils;
using static CheckMade.ChatBot.Logic.Workflows.UserAuthWorkflow.States;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsReceivedTokenSubmissionAttempt_AfterUserEnteredAnyText()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput> { basics.utils.GetValidTlgTextMessage() });
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);
        
        var actualState = await workflow.DetermineCurrentStateAsync(
            TestUserId_01, TestChatId_01, Operations);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }

    [Fact]
    public async Task DetermineCurrentStateAsync_OnlyConsidersInputs_SinceDeactivationOfLastTlgClientPortModeRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    
        // Depends on an 'expired' clientPortModeRole set up by default in the MockTlgClientPortModeRoleRepository 
        var tlgPastInputToBeIgnored = basics.utils.GetValidTlgTextMessage(
            TestUserId_02,
            TestChatId_03,
            SanitaryOpsAdmin1.Token,
            new DateTime(1999, 01, 05));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_02))
            .ReturnsAsync(new List<TlgInput> { tlgPastInputToBeIgnored });
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);
        
        var actualState = await workflow.DetermineCurrentStateAsync(
            TestUserId_02, TestChatId_03, Operations);
        
        Assert.Equal(ReadyToReceiveToken, actualState);
    }

    [Fact]
    // We know it's failed because a successful submission means no revisiting of the UserAuthWorkflow!
    public async Task DetermineCurrentStateAsync_ReturnsReceivedTokenSubmissionAttempt_AfterFailedAttempt()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput> { basics.utils.GetValidTlgTextMessage(text: "InvalidToken") });
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);
        
        var actualState = await workflow.DetermineCurrentStateAsync(
            TestUserId_01, TestChatId_01, Operations);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ReturnsCorrectErrorMessage_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        var nonExistingTokenInput = basics.utils.GetValidTlgTextMessage(
            text: InputValidator.GetTokenFormatExample());
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput> { nonExistingTokenInput });
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);
    
        var actualOutputs = await workflow.GetNextOutputAsync(nonExistingTokenInput);
        
        Assert.Equal("This is an unknown token. Try again...", GetFirstRawEnglish(actualOutputs));
    }

    [Fact]
    public async Task GetNextOutputAsync_ReturnsWarning_AndDeactivatesPreExisting_WhenTokenAlreadyHasActivePortModeRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputTokenWithPreExistingActivePortModeRole = 
            basics.utils.GetValidTlgTextMessage(text: SanitaryOpsAdmin1.Token);
        
        var preExistingActivePortModeRole = (await basics.mockPortModeRolesRepo.Object.GetAllAsync())
            .First(cpmr => 
                cpmr.Role.Token == SanitaryOpsAdmin1.Token &&
                cpmr.Mode == Operations);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput> { inputTokenWithPreExistingActivePortModeRole });

        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another {0} chat. 
                                       This will be the new {0} chat where you receive messages in your role {1} at {2}. 
                                       """;

        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);
        
        var actualOutputs = 
            await workflow.GetNextOutputAsync(inputTokenWithPreExistingActivePortModeRole);
        
        Assert.Equal(expectedWarning, GetFirstRawEnglish(actualOutputs));
        
        basics.mockPortModeRolesRepo.Verify(x => 
            x.UpdateStatusAsync(preExistingActivePortModeRole, DbRecordStatus.Historic));
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesPortModeRole_WithConfirmation_WhenValidTokenSubmitted_FromChatGroup()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputValidToken = basics.utils.GetValidTlgTextMessage(
            // Diverging userId and chatId = sent from a Tlgr Chat-Group (rather than a private chat with the bot)
            userId: TestUserId_03,
            chatId: TestChatId_08,
            text: SanitaryOpsInspector2.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_03))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        const string expectedConfirmation = """
                                            {0}, welcome to the CheckMade ChatBot!
                                            You have successfully authenticated as a {1} at live-event {2}.
                                            """;
        
        var expectedClientPortModeRoleAdded = new TlgClientPortModeRole(
            SanitaryOpsInspector2,
            new TlgClientPort(TestUserId_03, TestChatId_08),
            Operations,
            DateTime.UtcNow,
            Option<DateTime>.None());
        
        var actualClientPortModeRoleAdded = new List<TlgClientPortModeRole>(); 
        basics.mockPortModeRolesRepo
            .Setup(x => 
                x.AddAsync(It.IsAny<IEnumerable<TlgClientPortModeRole>>()))
            .Callback<IEnumerable<TlgClientPortModeRole>>(portModeRole => 
                actualClientPortModeRoleAdded = portModeRole.ToList());
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);

        var actualOutputs = await workflow.GetNextOutputAsync(inputValidToken);
        
        Assert.Equal(expectedConfirmation, GetFirstRawEnglish(actualOutputs));
        Assert.Equivalent(expectedClientPortModeRoleAdded.Role, actualClientPortModeRoleAdded[0].Role);
        Assert.Equivalent(expectedClientPortModeRoleAdded.ClientPort, actualClientPortModeRoleAdded[0].ClientPort);
        Assert.Equal(expectedClientPortModeRoleAdded.Mode, actualClientPortModeRoleAdded[0].Mode);
        Assert.Equivalent(expectedClientPortModeRoleAdded.Status, actualClientPortModeRoleAdded[0].Status);
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesPortModeRolesForAllModes_WhenValidTokenSubmitted_FromPrivateChat()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        const long privateChatUserAndChatId = TestUserId_03; 

        var inputValidToken = basics.utils.GetValidTlgTextMessage(
            userId: privateChatUserAndChatId,
            chatId: privateChatUserAndChatId,
            text: SanitaryOpsInspector2.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_03))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        var allModes = Enum.GetValues(typeof(InteractionMode)).Cast<InteractionMode>();
        
        var expectedClientPortModeRolesAdded = allModes.Select(im => 
            new TlgClientPortModeRole(
                SanitaryOpsInspector2,
                new TlgClientPort(privateChatUserAndChatId, privateChatUserAndChatId),
                im,
                DateTime.UtcNow,
                Option<DateTime>.None()))
            .ToList();

        var actualPortModeRoles = new List<TlgClientPortModeRole>();
        basics.mockPortModeRolesRepo
            .Setup(x =>
                x.AddAsync(It.IsAny<IEnumerable<TlgClientPortModeRole>>()))
            .Callback<IEnumerable<TlgClientPortModeRole>>(
                portModeRoles => actualPortModeRoles = portModeRoles.ToList());
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);

        await workflow.GetNextOutputAsync(inputValidToken);
        
        basics.mockPortModeRolesRepo.Verify(x => x.AddAsync(
            It.IsAny<IEnumerable<TlgClientPortModeRole>>()));

        for (var i = 0; i < expectedClientPortModeRolesAdded.Count; i++)
        {
            Assert.Equivalent(expectedClientPortModeRolesAdded[i].ClientPort, actualPortModeRoles[i].ClientPort);
            Assert.Equivalent(expectedClientPortModeRolesAdded[i].Role, actualPortModeRoles[i].Role);
            Assert.Equivalent(expectedClientPortModeRolesAdded[i].Mode, actualPortModeRoles[i].Mode);
            Assert.Equivalent(expectedClientPortModeRolesAdded[i].Status, actualPortModeRoles[i].Status);
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
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        var badTokenInput = basics.utils.GetValidTlgTextMessage(text: badToken);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                badTokenInput
            });
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortModeRolesRepo.Object);
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.Equal("Bad token format! Try again...", GetFirstRawEnglish(actualOutputs));
    }

    private static (ITestUtils utils, 
        Mock<ITlgClientPortModeRoleRepository> mockPortModeRolesRepo, 
        IRoleRepository mockRoleRepo, 
        DateTime baseDateTime) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(),
            sp.GetRequiredService<Mock<ITlgClientPortModeRoleRepository>>(),
            sp.GetRequiredService<IRoleRepository>(),
            DateTime.UtcNow);
}