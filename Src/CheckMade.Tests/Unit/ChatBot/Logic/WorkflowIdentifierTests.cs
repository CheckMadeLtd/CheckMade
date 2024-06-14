using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public class WorkflowIdentifierTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task IdentifyCurrentWorkflow_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var portWithoutRole = new TlgClientPort(2468L, 13563897L);
        var inputFromUnauthenticatedUser = utils.GetValidTlgTextMessage(
            portWithoutRole.UserId, portWithoutRole.ChatId);
    
        var workflow = await workflowIdentifier.IdentifyAsync(inputFromUnauthenticatedUser);
        
        Assert.True(workflow.GetValueOrThrow() is UserAuthWorkflow);
    }
}
