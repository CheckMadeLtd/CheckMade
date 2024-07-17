using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
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
        
        List<TlgInput> recentLocationHistory = [
            basics.inputGenerator.GetValidTlgInputLocationMessage(
                GetLocationFarFromAnySaniCleanSphere(),
                dateTime: DateTime.UtcNow.AddSeconds(-10)),
            basics.inputGenerator.GetValidTlgInputLocationMessage(
                GetLocationNearSaniCleanSphere())];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: recentLocationHistory);

        var currentInput = 
            basics.inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue);
        
        const string expectedOutput = "Please confirm: are you at {0} '{1}'?";
        var expectedNewState = basics.glossary.GetId(typeof(NewIssueInitialSphereKnown));
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
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetResponseAsync_PromptsSphereSelection_WhenNoLocationHistoryOrNotNearSphere(
        bool hasLocationHistory)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        var basics = TestUtils.GetBasicWorkflowTestingServices(_services);
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> recentLocationHistory = [];
        
        if (hasLocationHistory)
        {
            recentLocationHistory.Add(
                basics.inputGenerator.GetValidTlgInputLocationMessage(
                    GetLocationFarFromAnySaniCleanSphere(),
                    dateTime: DateTime.UtcNow));
        }
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: recentLocationHistory);

        var currentInput = 
            basics.inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue);
        
        const string expectedOutput = "Please select a {0}:";
        var expectedNewState = basics.glossary.GetId(typeof(NewIssueInitialSphereUnknown));
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
    public async Task GetResponseAsync_PromptsIssueTypeSelection_WhenSphereConfirmed()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        var basics = TestUtils.GetBasicWorkflowTestingServices(_services);
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> interactiveHistory = [
            basics.inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                (int)OperationsBotCommands.NewIssue,
                resultantWorkflowInfo: new ResultantWorkflowInfo(
                    basics.glossary.GetId(typeof(NewIssueWorkflow)),
                    basics.glossary.GetId(typeof(NewIssueInitialSphereUnknown))))];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory);

        var currentInput =
            basics.inputGenerator.GetValidTlgInputTextMessage(
                text: Sphere1_AtX2024.Name);

        const string expectedOutput = "Please select the type of issue:";
        var expectedNewState = 
            basics.glossary.GetId(typeof(NewIssueSphereConfirmed));
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
    
    private static Geo GetLocationNearSaniCleanSphere() =>
        new(
            Sphere1_Location.Latitude + 0.00001, // ca. 1 meter off
            Sphere1_Location.Longitude + 0.00001,
            Option<double>.None());

    private static Geo GetLocationFarFromAnySaniCleanSphere() =>
        new(
            Sphere1_Location.Latitude + 1, // ca. 100km off
            Sphere1_Location.Longitude,
            Option<double>.None());
}