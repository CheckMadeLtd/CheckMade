using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;

public class NewIssueWorkflowTests
{
    private ServiceProvider? _services;

    [Fact]
    public void DetermineCurrentState_Returns_InitialSphereUnknown_WhenNewIssueCommandWithNoRecentLocationUpdate()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> inputHistoryWithoutRecentLocationUpdate = [
            inputGenerator.GetValidTlgInputTextMessage(),
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, (int)OperationsBotCommands.NewIssue)];

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistoryWithoutRecentLocationUpdate);
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

        var actualState =
            workflow.DetermineCurrentState(inputHistoryWithoutRecentLocationUpdate);
        
        Assert.Equal(
            NewIssueWorkflow.States.InitialSphereUnknown,
            actualState);
    }
    
    [Fact]
    public void DetermineCurrentState_Returns_InitialSphereKnown_WhenNewIssueCommandWithRecentLocationUpdate()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();

        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        List<TlgInput> inputHistoryWithRecentLocationUpdate = [
            inputGenerator.GetValidTlgInputTextMessage(), 
            inputGenerator.GetValidTlgInputLocationMessage(
                13.04, -21.005, Option<float>.None()),
            inputGenerator.GetValidTlgInputCommandMessage(
                tlgAgent.Mode, (int)OperationsBotCommands.NewIssue)];

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: inputHistoryWithRecentLocationUpdate);
        var workflow = services.GetRequiredService<INewIssueWorkflow>();

        var actualState = 
            workflow.DetermineCurrentState(inputHistoryWithRecentLocationUpdate);

        Assert.Equal(NewIssueWorkflow.States.InitialSphereKnown, actualState);
    }
}