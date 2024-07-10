using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core.Trades.Types;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public class NewIssueWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public void DetermineCurrentState_ReturnsInitialTradeUnknown_OnNewIssueFromRoleWithMultipleTrades()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        // var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        
    }
    
    [Fact(Skip = "Not implemented")]
    public void DetermineCurrentState_ReturnsInitialSphereUnknown_OnNewIssueWithoutRecentLocationUpdates()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> interactiveHistory = [
            inputGenerator.GetValidTlgInputTextMessage(),
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, (int)OperationsBotCommands.NewIssue)];

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory);
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

        var actualState =
            workflow.DetermineCurrentState(
                interactiveHistory, 
                []);
        
        Assert.Equal(
            NewIssueWorkflow.States.Initial_SphereUnknown,
            actualState);
    }

    [Fact(Skip = "Not implemented")]
    public void DetermineCurrentState_ReturnsInitialSphereUnknown_OnNewIssue_WithLocationUpdateNotNearAnySphere()
    {
        throw new NotImplementedException();
    }

    [Fact(Skip = "Not implemented")]
    public void DetermineCurrentState_ReturnsInitialSphereKnown_OnNewIssueForSanitaryCleaning_WithLocationUpdateNearAnySphere()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> interactiveHistory = [
            inputGenerator.GetValidTlgInputTextMessage(), 
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, (int)OperationsBotCommands.NewIssue)];

        var nearSphere1LocationLatitude = 
            Sphere1_Location.Latitude + TradeSaniClean.SphereNearnessThresholdInMeters - 1;
        var nearSphere1LocationLongitude =
            Sphere1_Location.Longitude + TradeSaniClean.SphereNearnessThresholdInMeters - 1;
        var farFromSphere1LocationLatitude = 
            Sphere1_Location.Latitude + TradeSaniClean.SphereNearnessThresholdInMeters + 1;
        var farFromSphere1LocationLongitude = 
            Sphere1_Location.Longitude + TradeSaniClean.SphereNearnessThresholdInMeters + 1;
        
        List<TlgInput> recentLocationHistory = [
            inputGenerator.GetValidTlgInputLocationMessage(
                farFromSphere1LocationLatitude,
                farFromSphere1LocationLongitude,
                Option<float>.None(),
                dateTime: DateTime.UtcNow.AddSeconds(-10)),
            inputGenerator.GetValidTlgInputLocationMessage(
                nearSphere1LocationLatitude,
                nearSphere1LocationLongitude,
                Option<float>.None())];
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: interactiveHistory);
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

        var actualState = 
            workflow.DetermineCurrentState(
                interactiveHistory,
                recentLocationHistory);

        Assert.Equal(NewIssueWorkflow.States.Initial_SphereKnown, actualState);
    }
}