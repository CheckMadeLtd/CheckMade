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

    // ToDo Add tests: 
    // Success auth message also shows role and event and name "Lukas, you have authenticated as SanitaryAdmin in Event xy" 

    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsCorrectState_AfterUserConfirmedReadinessForTokenEntry()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate)
            });

        var mockPortToRoleRepo = _services.GetRequiredService<ITlgClientPortToRoleMapRepository>();
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, mockPortToRoleRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(ReadyToEnterToken, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_ReturnsCorrectState_AfterUserSubmittedToken()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();

        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                utils.GetValidTlgCallbackQueryForControlPrompts(Submit)
            });
        
        var mockPortToRoleRepo = _services.GetRequiredService<ITlgClientPortToRoleMapRepository>();
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, mockPortToRoleRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_01, TestChatId_01);
        
        Assert.Equal(TokenSubmitted, actualState);
    }
    
    [Fact]
    public async Task DetermineCurrentStateAsync_OnlyConsidersInputs_SinceEndDateOfLastTlgClientPortToRoleMapping()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    
        // Uses a set up 'expired' mapping in MockTlgClientPortToRoleMapRepository 
        var tlgPastSubmitInputToBeIgnored = utils.GetValidTlgCallbackQueryForControlPrompts(
            Submit, 
            TestUserId_02,
            TestChatId_03,
            new DateTime(1999, 01, 05));
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_02))
            .ReturnsAsync(new List<TlgInput>
            {
                tlgPastSubmitInputToBeIgnored,
                utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate)
            });
        
        var mockPortToRoleRepo = _services.GetRequiredService<ITlgClientPortToRoleMapRepository>();
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, mockPortToRoleRepo);
        
        var actualState = await workflow.DetermineCurrentStateAsync(TestUserId_02, TestChatId_03);
        
        Assert.Equal(ReadyToEnterToken, actualState);
    }
    
    // ToDo: write test for when user clicks 'cancel' -> back to Virgin!
    
    // ToDo: write test that, for well-formatted token, checks a) if it exists and b) if it is already used (-> warning but success)
    
    [Theory]
    [InlineData("5JFU")]
    [InlineData(" ")]
    [InlineData(" some text with trailing spaces and \n line break ")]
    [InlineData("")]
    public async Task GetNextOutputAsync_ReturnsFailedResult_WhenFormatOfEnteredTokenIsInvalid(
        string badToken)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
        
        mockTlgInputsRepo
            .Setup(repo => repo.GetAllAsync(TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate),
                utils.GetValidTlgCallbackQueryForControlPrompts(Submit)
            });
        
        var mockPortToRoleRepo = _services.GetRequiredService<ITlgClientPortToRoleMapRepository>();
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, mockPortToRoleRepo);
        var badTokenInput = utils.GetValidTlgTextMessage(text: badToken);
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.True(actualOutputs.IsError);
    }
}