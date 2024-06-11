using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.UserInteraction;
using CheckMade.Telegram.Logic.Workflows;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CheckMade.Tests.Unit.Telegram.Logic.Workflows;

public class UserAuthWorkflowTests
{
    private ServiceProvider? _services;

    // ToDo Add tests: 
    // 1. ReadyToEnterToken status
    // 2. Success auth message also shows role and event and name "Lukas, you have authenticated as SanitaryAdmin in Event xy" 
    
    // [Fact]
    // public async Task GetNextOutputAsync_
    
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
            .Setup(x => x.GetAllAsync(ITestUtils.TestUserId_01))
            .ReturnsAsync(new List<TlgInput>
            {
                utils.GetValidTlgCallbackQueryForControlPrompts(ControlPrompts.Authenticate),
                utils.GetValidTlgCallbackQueryForControlPrompts(ControlPrompts.Submit)
            });
        
        var workflow = new UserAuthWorkflow(mockTlgInputsRepo.Object);
        var badTokenInput = utils.GetValidTlgTextMessage(text: badToken);
    
        var actualOutputs = await workflow.GetNextOutputAsync(badTokenInput);
        
        Assert.True(actualOutputs.IsError);
    }
}