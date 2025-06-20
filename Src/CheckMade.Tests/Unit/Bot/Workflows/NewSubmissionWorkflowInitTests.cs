using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Core.GIS;
using CheckMade.Abstract.Domain.Model.Core.Trades;
using CheckMade.Bot.Workflows.Ops.NewSubmission;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.A_Init;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.B_Details;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using General.Utils.FpExtensions.Monads;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Bot.Workflows;

public sealed class NewSubmissionWorkflowInitTests
{
    private ServiceProvider? _services;
    
    [Fact]
    public async Task GetResponseAsync_PromptsTradeSelection_ForLiveEventAdminRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var basics = TestUtils.GetBasicWorkflowTestingServices(_services);
        var agent = PrivateBotChat_Operations;
        var currentRole = LiveEventAdmin_DanielEn_X2024;
        Assert.True(currentRole.RoleType.GetTradeInstance().IsNone);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories();

        var currentInput = basics.inputGenerator.GetValidInputCommandMessage(
            agent.Mode,
            (int)OperationsBotCommands.NewSubmission,
            roleSpecified: currentRole);

        const string expectedOutput = "Please select a Trade:";
        var expectedNewState = basics.glossary.GetId(typeof(INewSubmissionTradeSelection));
        var workflow = services.GetRequiredService<NewSubmissionWorkflow>();

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
        var agent = PrivateBotChat_Operations;
        
        List<Input> recentLocationHistory = [
            basics.inputGenerator.GetValidInputLocationMessage(
                GetLocationFarFromAnySanitarySphere(),
                dateTime: DateTimeOffset.UtcNow.AddSeconds(-10)),
            basics.inputGenerator.GetValidInputLocationMessage(
                GetLocationNearSanitarySphere())];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: recentLocationHistory);

        var currentInput = 
            basics.inputGenerator.GetValidInputCommandMessage(
                agent.Mode, 
                (int)OperationsBotCommands.NewSubmission);
        
        const string expectedOutput = "Please confirm: are you at '{0}'?";
        var expectedNewState = basics.glossary.GetId(typeof(INewSubmissionSphereConfirmation<SanitaryTrade>));
        var workflow = services.GetRequiredService<NewSubmissionWorkflow>();

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
        var agent = PrivateBotChat_Operations;

        List<Input> recentLocationHistory = [];
        
        if (hasLocationHistory)
        {
            recentLocationHistory.Add(
                basics.inputGenerator.GetValidInputLocationMessage(
                    GetLocationFarFromAnySanitarySphere(),
                    dateTime: DateTimeOffset.UtcNow));
        }
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: recentLocationHistory);

        var currentInput = 
            basics.inputGenerator.GetValidInputCommandMessage(
                agent.Mode, 
                (int)OperationsBotCommands.NewSubmission);
        
        const string expectedOutput = "Please select a ";
        var expectedNewState = basics.glossary.GetId(typeof(INewSubmissionSphereSelection<SanitaryTrade>));
        var workflow = services.GetRequiredService<NewSubmissionWorkflow>();

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
    public async Task GetResponseAsync_PromptsSubmissionTypeSelection_WhenSphereConfirmed()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        var basics = TestUtils.GetBasicWorkflowTestingServices(_services);
        var agent = PrivateBotChat_Operations;

        List<Input> interactiveHistory = [
            basics.inputGenerator.GetValidInputCommandMessage(
                agent.Mode,
                (int)OperationsBotCommands.NewSubmission,
                resultantWorkflowState: new ResultantWorkflowState(
                    basics.glossary.GetId(typeof(NewSubmissionWorkflow)),
                    basics.glossary.GetId(typeof(INewSubmissionSphereSelection<SanitaryTrade>))))];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory);

        var currentInput =
            basics.inputGenerator.GetValidInputTextMessage(
                text: NewSubmissionUtils.SphereLabelComposer(Sphere1_AtX2024));

        const string expectedOutput = "Please select the type of submission:";
        var expectedNewState = 
            basics.glossary.GetId(typeof(INewSubmissionTypeSelection<SanitaryTrade>));
        var workflow = services.GetRequiredService<NewSubmissionWorkflow>();

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