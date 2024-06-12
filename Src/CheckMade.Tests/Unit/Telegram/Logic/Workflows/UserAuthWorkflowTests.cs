using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Utils.Generic;
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
    public async Task DetermineCurrentStateAsync_ReturnsReadyToReceiveToken_AfterUserConfirmedReadiness()
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

        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portRolesRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(ReadyToReceiveToken, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsReceivedTokenSubmissionAttempt_AfterUserEnteredAnyText()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                basics.utils.GetValidTlgTextMessage()
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portRolesRepo);
        
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
        var tlgPastInputToBeIgnored = basics.utils.GetValidTlgCallbackQueryForControlPrompts(
            Authenticate,
            TestUserId_02,
            TestChatId_03,
            new DateTime(1999, 01, 05));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_02))
            .ReturnsAsync(new List<TlgInput> { tlgPastInputToBeIgnored });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portRolesRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_02, TestChatId_03);
        
        Assert.Equal(Virgin, actualState);
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
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                basics.utils.GetValidTlgTextMessage()
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portRolesRepo);
        
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
            text: InputValidator.GetTokenFormatExample(),
            dateTime: basics.baseDateTime.AddSeconds(1));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(
                    Authenticate, dateTime: basics.baseDateTime),
                nonExistingTokenInput
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portRolesRepo);
    
        var actualOutputs = await workflow.GetNextOutputAsync(nonExistingTokenInput);
        
        Assert.Equal("This is an unknown token. Try again...", GetFirstRawEnglish(actualOutputs));
    }

    // [Fact]
    // public async Task GetNextOutputAsync_ReturnsWarningMessage_WhenSubmittedTokenHasActivePortRole()
    // {
    //     // this requires looking up in the portRoleRepo whether an active portRole exists
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     var basics = GetBasicTestingServices(_services);
    //     var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    //
    //     mockTlgInputsRepo
    //         .Setup(repo => repo.GetAllAsync(TestUserId_01))
    //         .ReturnsAsync(new List<TlgInput>
    //         {
    //             basics.utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
    //             //
    //         });
    // }

    [Fact]
    public async Task GetNextOutputAsync_CreatesPortRole_AndReturnsDetailedConfirmation_WhenSubmittedTokenValid()
    {
        // ToDo: probably verify that 'Add' was called on the mockedPortRolesRepo!
        
        // Success auth message also shows role and event and name "Lukas, you have authenticated as SanitaryAdmin in Event xy"
        // This now requires implementing additional fields in RoleRepo as well as LiveEventRepo. 
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
        
        var badTokenInput = basics.utils.GetValidTlgTextMessage(
            text: badToken,
            dateTime: basics.baseDateTime.AddSeconds(1));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                basics.utils.GetValidTlgCallbackQueryForControlPrompts(
                    Authenticate, dateTime: basics.baseDateTime),
                badTokenInput
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, basics.roleRepo, basics.portRolesRepo);
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.Equal("Bad token format! The correct format is: '{0}'", GetFirstRawEnglish(actualOutputs));
    }

    private static (ITestUtils utils, ITlgClientPortRoleRepository portRolesRepo, IRoleRepository roleRepo, DateTime baseDateTime) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(),
            sp.GetRequiredService<ITlgClientPortRoleRepository>(),
            sp.GetRequiredService<IRoleRepository>(),
            DateTime.Now);
}