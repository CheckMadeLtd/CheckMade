using CheckMade.Common.Model.Telegram;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Logic.Workflows;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class WorkflowIdentifierTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task IdentifyCurrentWorkflow_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var portUnmappedToRole = new TlgClientPort(2468L, 13563897L);
        var inputFromUnauthenticatedUser = utils.GetValidTlgTextMessage(
            portUnmappedToRole.UserId, portUnmappedToRole.ChatId);
    
        var workflow = await workflowIdentifier.IdentifyAsync(inputFromUnauthenticatedUser);
        
        Assert.True(workflow.GetValueOrThrow() is UserAuthWorkflow);
    }
}
