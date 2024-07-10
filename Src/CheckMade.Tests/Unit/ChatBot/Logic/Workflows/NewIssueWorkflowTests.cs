using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;
using CheckMade.Common.Utils.GIS;
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

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory
        );
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

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

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory);
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

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

        List<TlgInput> interactiveHistory = [
            inputGenerator.GetValidTlgInputTextMessage(), 
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, 
                (int)OperationsBotCommands.NewIssue)];

        var nearSphere1LocationLatitude = 
            Sphere1_Location.Latitude;
        var nearSphere1LocationLongitude =
            Sphere1_Location.Longitude;
        
        var farFromSphere1LocationLatitude = 
            Sphere1_Location.Latitude + 1.0;
        var farFromSphere1LocationLongitude = 
            Sphere1_Location.Longitude + 1.0;
        
        Assert.True(
            new Geo(
                    farFromSphere1LocationLatitude, 
                    farFromSphere1LocationLongitude,
                    Option<float>.None())
                .MetersAwayFrom(Sphere1_Location) 
            > SaniCleanTrade.SphereNearnessThresholdInMeters);
        
        List<TlgInput> recentLocationHistory = [
            inputGenerator.GetValidTlgInputLocationMessage(
                farFromSphere1LocationLatitude,
                farFromSphere1LocationLongitude,
                Option<float>.None(),
                dateTime: DateTime.UtcNow.AddSeconds(-10))];

        if (isNearSphere)
        {
            recentLocationHistory.Add(
                inputGenerator.GetValidTlgInputLocationMessage(
                    nearSphere1LocationLatitude,
                    nearSphere1LocationLongitude,
                    Option<float>.None()));
        }
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory);
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

        var actualState = 
            workflow.DetermineCurrentState(
                interactiveHistory,
                recentLocationHistory,
                X2024);

        Assert.Equal(expectedState, actualState);
    }
}