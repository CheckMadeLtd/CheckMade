using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.UserInteraction;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Logic.Workflows;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class InputProcessorTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData(InteractionMode.Operations)]
    [InlineData(InteractionMode.Communications)]
    [InlineData(InteractionMode.Notifications)]
    public async Task IdentifyCurrentWorkflow_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated(
        InteractionMode mode)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services, mode);
        var portUnmappedToRole = new TlgClientPort(2468L, 13563897L);
        var input = basics.utils.GetValidTlgTextMessage(
            portUnmappedToRole.UserId, portUnmappedToRole.ChatId);
    
        var workflow = await basics.processor.IdentifyCurrentWorkflowAsync(input);
        
        Assert.True(workflow.GetValueOrThrow() is UserAuthWorkflow);
    }
    
    private static (ITestUtils utils, IInputProcessor processor) 
        GetBasicTestingServices(IServiceProvider sp, InteractionMode mode) =>
            (sp.GetRequiredService<ITestUtils>(), 
                sp.GetRequiredService<IInputProcessorFactory>().GetInputProcessor(mode));
}
