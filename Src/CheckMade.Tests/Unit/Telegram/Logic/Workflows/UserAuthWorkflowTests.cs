using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Utils.Generic;
using CheckMade.Telegram.Logic.Workflows;
using CheckMade.Tests.Startup;
using static CheckMade.Tests.ITestUtils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static CheckMade.Telegram.Logic.Workflows.UserAuthWorkflow.States;

namespace CheckMade.Tests.Unit.Telegram.Logic.Workflows;

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
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortRolesRepo.Object);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(ReceivedTokenSubmissionAttempt, actualState);
    }

    [Fact]
    public async Task DetermineCurrentStateAsync_OnlyConsidersInputs_SinceDeactivationOfLastTlgClientPortRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    
        // Depends on an 'expired' clientPortRole set up by default in the MockTlgClientPortRoleRepository 
        var tlgPastInputToBeIgnored = basics.utils.GetValidTlgTextMessage(
            TestUserId_02,
            TestChatId_03,
            SanitaryOpsAdmin1.Token,
            new DateTime(1999, 01, 05));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_02))
            .ReturnsAsync(new List<TlgInput> { tlgPastInputToBeIgnored });
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortRolesRepo.Object);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_02, TestChatId_03);
        
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
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortRolesRepo.Object);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
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
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortRolesRepo.Object);
    
        var actualOutputs = await workflow.GetNextOutputAsync(nonExistingTokenInput);
        
        Assert.Equal("This is an unknown token. Try again...", GetFirstRawEnglish(actualOutputs));
    }

    [Fact]
    public async Task GetNextOutputAsync_ReturnsWarningMessage_WhenSubmittedTokenAlreadyHasActivePortRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputTokenWithActivePortRole = basics.utils.GetValidTlgTextMessage(text: SanitaryOpsAdmin1.Token);
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput> { inputTokenWithActivePortRole });

        const string expectedWarning = """
                                       Warning: you were already authenticated with this token in another chat. 
                                       This will be the new chat where you receive messages in your role {0} at {1}. 
                                       """;
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortRolesRepo.Object);
        
        var actualOutputs = await workflow.GetNextOutputAsync(inputTokenWithActivePortRole);
        
        Assert.Equal(expectedWarning, GetFirstRawEnglish(actualOutputs));
    }

    [Fact]
    public async Task GetNextOutputAsync_CreatesPortRole_AndReturnsDetailedConfirmation_WhenSubmittedTokenValid()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        var inputValidToken = basics.utils.GetValidTlgTextMessage(
            userId: TestUserId_03,
            chatId: TestChatId_08,
            text: SanitaryOpsInspector2.Token);

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_03))
            .ReturnsAsync(new List<TlgInput> { inputValidToken });

        const string expectedConfirmation = "{0}, you have successfully authenticated as a {1} at live-event {2}.";
        var expectedClientPortRoleAdded = new TlgClientPortRole(
            SanitaryOpsInspector2,
            new TlgClientPort(TestUserId_03, TestChatId_08),
            DateTime.Now,
            Option<DateTime>.None());
        
        TlgClientPortRole? actualClientPortRoleAdded = null; 
        basics.mockPortRolesRepo
            .Setup(x => x.AddAsync(It.IsAny<TlgClientPortRole>()))
            .Callback<TlgClientPortRole>(portRole => actualClientPortRoleAdded = portRole);
        
        var workflow = await UserAuthWorkflow.CreateAsync(
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortRolesRepo.Object);

        var actualOutputs = await workflow.GetNextOutputAsync(inputValidToken);
        
        Assert.Equal(expectedConfirmation, GetFirstRawEnglish(actualOutputs));
        Assert.Equivalent(expectedClientPortRoleAdded.Role, actualClientPortRoleAdded!.Role);
        Assert.Equivalent(expectedClientPortRoleAdded.ClientPort, actualClientPortRoleAdded!.ClientPort);
        Assert.Equivalent(expectedClientPortRoleAdded.Status, actualClientPortRoleAdded!.Status);
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
            mockTlgInputsRepo.Object, basics.mockRoleRepo, basics.mockPortRolesRepo.Object);
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.Equal("Bad token format! The correct format is: '{0}'", GetFirstRawEnglish(actualOutputs));
    }

    private static (ITestUtils utils, 
        Mock<ITlgClientPortRoleRepository> mockPortRolesRepo, 
        IRoleRepository mockRoleRepo, 
        DateTime baseDateTime) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(),
            sp.GetRequiredService<Mock<ITlgClientPortRoleRepository>>(),
            sp.GetRequiredService<IRoleRepository>(),
            DateTime.Now);
}