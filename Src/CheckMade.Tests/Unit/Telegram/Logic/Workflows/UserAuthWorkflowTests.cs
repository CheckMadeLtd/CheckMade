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
    
    // [Fact]
    // public async Task DetermineCurrentStateAsync_OnlyConsidersInputs_SinceEndDateOfLastTlgClientPortToRoleMapping()
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     var utils = _services.GetRequiredService<ITestUtils>();
    //     var mockTlgInputsRepo = new Mock<ITlgInputRepository>();
    //
    //     // See expired mapping in MockTlgClientPortToRoleMapRepository 
    //     var tlgPastInputToBeIgnored = utils.GetValidTlgCallbackQueryForControlPrompts(
    //         Submit, 
    //         ITestUtils.TestUserId_02,
    //         ITestUtils.TestChatId_03,
    //         new DateTime(1999, 01, 05));
    //     
    //     mockTlgInputsRepo
    //         .Setup(repo => repo.GetAllAsync(ITestUtils.TestUserId_01))
    //         .ReturnsAsync(new List<TlgInput>
    //         {
    //             tlgPastInputToBeIgnored,
    //             utils.GetValidTlgCallbackQueryForControlPrompts(Authenticate)
    //         });
    //     
    //     var mockPortToRoleRepo = _services.GetRequiredService<ITlgClientPortToRoleMapRepository>();
    //     var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object, mockPortToRoleRepo);
    //     
    //     var actualState = await workflow.DetermineCurrentStateAsync(ITestUtils.TestUserId_01);
    //     
    //     Assert.Equal(TokenSubmitted, actualState);
    // }
    
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