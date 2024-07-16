// using CheckMade.ChatBot.Logic.Workflows.Concrete;
// using CheckMade.Common.Interfaces.ChatBot.Logic;
// using CheckMade.Common.Model.ChatBot.Input;
// using CheckMade.Common.Model.ChatBot.UserInteraction;
// using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
// using CheckMade.Common.Model.Core;
// using CheckMade.Tests.Startup;
// using CheckMade.Tests.Utils;
// using Microsoft.Extensions.DependencyInjection;
// using static CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueWorkflow.States;
//
// namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;
//
// public class NewIssueWorkflowDetermineCurrentStateTests
// {
//     private ServiceProvider? _services;
//
//
//     [Theory]
//     [InlineData(Sphere1_AtX2024_Name, SphereConfirmed)]
//     [InlineData("Invalid sphere name", Initial_SphereUnknown)]
//     public void DetermineCurrentState_ReturnsCorrectState_WhenUserManuallyEntersSphere(
//         string enteredSphereName, Enum expectedState)
//     {
//         _services = new UnitTestStartup().Services.BuildServiceProvider();
//
//         var glossary = _services.GetRequiredService<IDomainGlossary>();
//         var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
//         var tlgAgent = PrivateBotChat_Operations;
//         var workflowId = glossary.IdAndUiByTerm[Dt(typeof(NewIssueWorkflow))].callbackId;
//     
//         List<TlgInput> recentLocationHistory = [
//             inputGenerator.GetValidTlgInputLocationMessage(
//                 GetLocationFarFromAnySaniCleanSphere(),
//                 dateTime: DateTime.UtcNow)];
//     
//         List<TlgInput> interactiveHistory = [
//             inputGenerator.GetValidTlgInputCommandMessage(
//                 tlgAgent.Mode,
//                 (int)OperationsBotCommands.NewIssue,
//                 resultantWorkflowInfo: new ResultantWorkflowInfo(
//                     workflowId,
//                     Initial_SphereUnknown)),
//             inputGenerator.GetValidTlgInputTextMessage(
//                 text: enteredSphereName)];
//
//         var workflow = _services.GetRequiredService<INewIssueWorkflow>();
//
//         var actualState =
//             workflow.DetermineCurrentState(
//                 interactiveHistory,
//                 recentLocationHistory,
//                 X2024);
//         
//         Assert.Equal(expectedState, actualState);
//     }
//
// }