using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public class NewIssueWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public void DetermineCurrentState_ReturnsInitialTradeUnknown_OnNewIssueFromLiveEventAdminRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> interactiveHistory = [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue,
                roleSpecified: LiveEventAdmin_DanielEn_X2024)];

        var workflow = _services.GetRequiredService<INewIssueWorkflow>();

        var actualState =
            workflow.DetermineCurrentState(
                interactiveHistory,
                [],
                X2024);
        
        Assert.Equal(
            NewIssueWorkflow.States.Initial_TradeUnknown,
            actualState);
    }
    
    [Fact]
    public void DetermineCurrentState_ReturnsInitialSphereUnknown_OnNewIssueWithoutRecentLocationUpdates()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> interactiveHistory = [
            inputGenerator.GetValidTlgInputTextMessage(),
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue)];

        var workflow = _services.GetRequiredService<INewIssueWorkflow>();

        var actualState =
            workflow.DetermineCurrentState(
                interactiveHistory, 
                [],
                X2024);
        
        Assert.Equal(
            NewIssueWorkflow.States.Initial_SphereUnknown,
            actualState);
    }

    [Theory]
    [InlineData(true, NewIssueWorkflow.States.Initial_SphereKnown)]
    [InlineData(false, NewIssueWorkflow.States.Initial_SphereUnknown)]
    public void DetermineCurrentState_ReturnsCorrectInitialSphereState_OnNewIssueForSaniClean(
        bool isNearSphere, Enum expectedState)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> recentLocationHistory = [
            inputGenerator.GetValidTlgInputLocationMessage(
                GetLocationFarFromAnySaniCleanSphere(),
                dateTime: DateTime.UtcNow.AddSeconds(-10))];

        if (isNearSphere)
        {
            recentLocationHistory.Add(
                inputGenerator.GetValidTlgInputLocationMessage(
                    GetLocationNearSaniCleanSphere()));
        }
        
        List<TlgInput> interactiveHistory = [
            inputGenerator.GetValidTlgInputTextMessage(), 
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue)];

        var workflow = _services.GetRequiredService<INewIssueWorkflow>();

        var actualState = 
            workflow.DetermineCurrentState(
                interactiveHistory,
                recentLocationHistory,
                X2024);

        Assert.Equal(expectedState, actualState);
    }

    [Fact]
    public void DetermineCurrentState_ReturnsSphereConfirmed_WhenUserConfirmsAutomaticNearSphere()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var glossary = _services.GetRequiredService<IDomainGlossary>();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var workflowId = glossary.IdAndUiByTerm[Dt(typeof(NewIssueWorkflow))].callbackId;
    
        List<TlgInput> recentLocationHistory = [
            inputGenerator.GetValidTlgInputLocationMessage(
                GetLocationNearSaniCleanSphere(),
                dateTime: DateTime.UtcNow)];
    
        List<TlgInput> interactiveHistory = [
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode,
                (int)OperationsBotCommands.NewIssue,
                resultantWorkflowInfo: new ResultantWorkflowInfo(
                    workflowId,
                    NewIssueWorkflow.States.Initial_SphereKnown)),
            inputGenerator.GetValidTlgInputCallbackQueryForControlPrompts(
                ControlPrompts.Yes)];

        var workflow = _services.GetRequiredService<INewIssueWorkflow>();

        var actualState =
            workflow.DetermineCurrentState(
                interactiveHistory,
                recentLocationHistory,
                X2024);
        
        Assert.Equal(
            NewIssueWorkflow.States.SphereConfirmed, 
            actualState);
    }

    [Fact]
    public void DetermineCurrentState_ReturnsSphereConfirmed_WhenUserManuallyChoseSphere()
    {
        
    }

    private Geo GetLocationNearSaniCleanSphere() =>
        new Geo(
            Sphere1_Location.Latitude + 0.00001, // ca. 1 meter off
            Sphere1_Location.Longitude + 0.00001,
            Option<float>.None());

    private Geo GetLocationFarFromAnySaniCleanSphere() =>
        new Geo(
            Sphere1_Location.Latitude + 1, // ca. 100km off
            Sphere1_Location.Longitude,
            Option<float>.None());
}