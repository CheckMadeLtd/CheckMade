using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Logic;

public class WorkflowIdentifierTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task IdentifyAsync_ReturnsUserAuthWorkflow_WhenUserNotAuthenticated()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();
        
        var portWithoutRole = new TlgClientPort(2468L, 13563897L, InteractionMode.Operations);
        var inputFromUnauthenticatedUser = utils.GetValidTlgInputTextMessage(
            portWithoutRole.UserId, portWithoutRole.ChatId);
    
        var workflow = await workflowIdentifier.IdentifyAsync(inputFromUnauthenticatedUser);
        
        Assert.True(workflow.GetValueOrThrow() is UserAuthWorkflow);
    }

    [Fact]
    public async Task IdentifyAsync_ReturnsLanguageSettingWorkflow_OnCorrespondingBotCommand()
    {
        // ToDo: need to fix probably by registering a tlgInputRepo with that message contained, 
        // since the workflowIdent now relies on inputRepo !! 
        
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var utils = _services.GetRequiredService<ITestUtils>();
        var workflowIdentifier = _services.GetRequiredService<IWorkflowIdentifier>();

        var inputWithSettingsBotCommand = utils.GetValidTlgInputCommandMessage(
            InteractionMode.Operations, (int)OperationsBotCommands.Settings);

        var workflow = await workflowIdentifier.IdentifyAsync(inputWithSettingsBotCommand);
        
        Assert.True(workflow.GetValueOrThrow() is LanguageSettingWorkflow);
    }
}
