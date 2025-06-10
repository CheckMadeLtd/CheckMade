using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.B_Details;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public sealed class NewIssueWorkflowInitTests
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
        var expectedNewState = basics.glossary.GetId(typeof(INewIssueTradeSelection));
        var workflow = services.GetRequiredService<NewIssueWorkflow>();

        var actualResponse =
            await workflow.GetResponseAsync(currentInput);
        
        Assert.Equal(
            expectedOutput,
            actualResponse.GetValueOrThrow().Output.GetFirstRawEnglish());
        
        Assert.Equal(
            expectedNewState,
            actualResponse.GetValueOrThrow().NewStateId.GetValueOrThrow());
    }

    [Fact]
    public async Task GetResponseAsync_PromptsSphereConfirmation_WhenUserIsNearSphere()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        var basics = TestUtils.GetBasicWorkflowTestingServices(_services);
        var tlgAgent = PrivateBotChat_Operations;
        
        List<TlgInput> recentLocationHistory = [
            basics.inputGenerator.GetValidTlgInputLocationMessage(
                GetLocationFarFromAnySanitarySphere(),
                dateTime: DateTimeOffset.UtcNow.AddSeconds(-10)),
            basics.inputGenerator.GetValidTlgInputLocationMessage(
                GetLocationNearSanitarySphere())];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: recentLocationHistory);

        var currentInput = 
            basics.inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue);
        
        const string expectedOutput = "Please confirm: are you at '{0}'?";
        var expectedNewState = basics.glossary.GetId(typeof(INewIssueSphereConfirmation<SanitaryTrade>));
        var workflow = services.GetRequiredService<NewIssueWorkflow>();

        var actualResponse =
            await workflow.GetResponseAsync(currentInput);
        
        Assert.Equal(
            expectedOutput,
            actualResponse.GetValueOrThrow().Output.GetFirstRawEnglish());
        
        Assert.Equal(
            expectedNewState,
            actualResponse.GetValueOrThrow().NewStateId.GetValueOrThrow());
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
                    GetLocationFarFromAnySanitarySphere(),
                    dateTime: DateTimeOffset.UtcNow));
        }
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: recentLocationHistory);

        var currentInput = 
            basics.inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue);
        
        const string expectedOutput = "Please select a ";
        var expectedNewState = basics.glossary.GetId(typeof(INewIssueSphereSelection<SanitaryTrade>));
        var workflow = services.GetRequiredService<NewIssueWorkflow>();

        var actualResponse =
            await workflow.GetResponseAsync(currentInput);
        
        Assert.Equal(
            expectedOutput,
            actualResponse.GetValueOrThrow().Output.GetFirstRawEnglish());
        
        Assert.Equal(
            expectedNewState,
            actualResponse.GetValueOrThrow().NewStateId.GetValueOrThrow());
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
                resultantWorkflowState: new ResultantWorkflowState(
                    basics.glossary.GetId(typeof(NewIssueWorkflow)),
                    basics.glossary.GetId(typeof(INewIssueSphereSelection<SanitaryTrade>))))];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory);

        var currentInput =
            basics.inputGenerator.GetValidTlgInputTextMessage(
                text: Sphere1_AtX2024.Name);

        const string expectedOutput = "Please select the type of submission:";
        var expectedNewState = 
            basics.glossary.GetId(typeof(INewIssueTypeSelection<SanitaryTrade>));
        var workflow = services.GetRequiredService<NewIssueWorkflow>();

        var actualResponse =
            await workflow.GetResponseAsync(currentInput);
        
        Assert.Equal(
            expectedOutput,
            actualResponse.GetValueOrThrow().Output.GetFirstRawEnglish());
        
        Assert.Equal(
            expectedNewState,
            actualResponse.GetValueOrThrow().NewStateId.GetValueOrThrow());
    }
    
    private static Geo GetLocationNearSanitarySphere() =>
        new(
            Location_Dassel.Latitude + 0.00001, // ca. 1 meter off
            Location_Dassel.Longitude + 0.00001,
            Option<double>.None());

    private static Geo GetLocationFarFromAnySanitarySphere() =>
        new(
            Location_Dassel.Latitude + 1, // ca. 100km off
            Location_Dassel.Longitude,
            Option<double>.None());
}