using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using static CheckMade.Common.Model.Telegram.UserInteraction.ControlPrompts;
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
    public async Task DetermineCurrentStateAsync_ReturnsReadyToEnterTokenState_AfterUserConfirmedReadiness()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate)
            });

        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portToRoleMapRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(ReadyToEnterToken, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnTokenSubmittedState_AfterUserSubmittedToken()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Submit)
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portToRoleMapRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(TokenSubmittedWithUnknownOutcome, actualState);
    }

    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsVirginState_WhenUserCancelsTokenEntry()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Cancel)
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portToRoleMapRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(Virgin, actualState);
    }
        
    [Fact]
    public async Task DetermineCurrentStateAsync_OnlyConsidersInputs_SinceEndDateOfLastTlgClientPortToRoleMapping()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    
        // Uses a set up 'expired' mapping in MockTlgClientPortToRoleMapRepository 
        var tlgPastSubmitInputToBeIgnored = basics.utils.GetValidTlgCallbackQueryForControlPrompts(
            Submit, 
            TestUserId_02,
            TestChatId_03,
            new DateTime(1999, 01, 05));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_02))
            .ReturnsAsync(new List<TlgInput>
            {
                tlgPastSubmitInputToBeIgnored,
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate)
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portToRoleMapRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_02, TestChatId_03);
        
        Assert.Equal(ReadyToEnterToken, actualState);
    }

    [Fact]
    public async Task DetermineCurrentStateAsync_StaysInTokenSubmittedState_AfterFailedSubmissionAttempt()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Submit)
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portToRoleMapRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(TokenSubmittedWithUnknownOutcome, actualState);
    }
    
    [Fact]
    public async Task GetNextOutputAsync_ReturnsError_WhenSubmittedTokenNotExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Submit)
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portToRoleMapRepo);
        const string nonExistingToken = "AAAAAA";
        var nonExistingTokenInput = basics.utils.GetValidTlgTextMessage(text: nonExistingToken);
    
        var actualOutputs = await workflow.GetNextOutputAsync(nonExistingTokenInput);
        
        Assert.True(actualOutputs.IsError);
    }

    [Fact]
    public async Task GetNextOutputAsync_ReturnsWarningMessage_WhenSubmittedTokenIsCurrentlyMapped()
    {
        // ToDo: in this case, use a static Ui() in the workflow which can be reused here for verification of output.
    }

    [Fact]
    public async Task GetNextOutputAsync_MapsPortToRoleAndReturnsDetailedConfirmation_WhenSubmittedTokenValid()
    {
        // ToDo: probably verify that 'Add' was called on the mockedPortToRoleMap!
        
        // Success auth message also shows role and event and name "Lukas, you have authenticated as SanitaryAdmin in Event xy"
        // This now requires implementing additional fields in RoleRepo as well as LiveEventRepo. 
    }
    
    [Theory]
    [InlineData("5JFUX")]
    [InlineData(" ")]
    [InlineData(" some text with trailing spaces and \n line break ")]
    [InlineData("")]
    public async Task GetNextOutputAsync_ReturnsFailedResult_WhenFormatOfEnteredTokenIsInvalid(
        string badToken)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Submit)
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portToRoleMapRepo);
        var badTokenInput = basics.utils.GetValidTlgTextMessage(text: badToken);
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.True(actualOutputs.IsError);
    }

    private (ITestUtils utils, ITlgClientPortToRoleMapRepository portToRoleMapRepo, IRoleRepository roleRepo) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(),
            sp.GetRequiredService<ITlgClientPortToRoleMapRepository>(),
            sp.GetRequiredService<IRoleRepository>());
}