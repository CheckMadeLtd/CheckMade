using CheckMade.ChatBot.Logic;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public class NewIssueWorkflowInitTests
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task GetResponseAsync_PromptsTradeSelection_ForLiveEventAdminRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var basics = TestUtils.GetBasicWorkflowTestingServices(_services);
        var tlgAgent = PrivateBotChat_Operations;
        var currentRole = LiveEventAdmin_DanielEn_X2024;
        Assert.True(currentRole.RoleType.GetTradeInstance().IsNone);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories();

        var currentInput = basics.inputGenerator.GetValidTlgInputCommandMessage(
            tlgAgent.Mode,
            (int)OperationsBotCommands.NewIssue,
            roleSpecified: currentRole);

        const string expectedOutput = "Please select a Trade:";
        var expectedNewState = basics.glossary.GetId(typeof(NewIssueInitialTradeUnknown));
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

        var actualOutput =
            await workflow.GetResponseAsync(currentInput);
        
        Assert.Equal(
            expectedOutput,
            TestUtils.GetFirstRawEnglish(actualOutput.GetValueOrThrow().Output));
        
        Assert.Equal(
            expectedNewState,
            actualOutput.GetValueOrThrow().NewState.GetValueOrThrow());
    }

    [Fact]
    public async Task GetResponseAsync_PromptsSphereConfirmation_WhenUserIsNearSphere()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var basics = TestUtils.GetBasicWorkflowTestingServices(_services);
        var tlgAgent = PrivateBotChat_Operations;
    }
}