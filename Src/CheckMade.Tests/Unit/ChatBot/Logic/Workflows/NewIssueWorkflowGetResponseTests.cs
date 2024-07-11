// using CheckMade.ChatBot.Logic.Workflows.Concrete;
// using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
// using CheckMade.Tests.Startup;
// using CheckMade.Tests.Utils;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace CheckMade.Tests.Unit.ChatBot.Logic.Workflows;
//
// public class NewIssueWorkflowGetResponseTests
// {
//     private ServiceProvider? _services;
//
//     [Fact]
//     public async Task GetResponseAsync_PromptsTradeSelection_forState_Initial_TradeUnknown()
//     {
//         _services = new UnitTestStartup().Services.BuildServiceProvider();
//
//         var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
//         var tlgAgent = PrivateBotChat_Operations;
//         var currentRole = LiveEventAdmin_DanielEn_X2024;
//         Assert.True(currentRole.RoleType.GetTradeInstance().IsNone);
//
//         var serviceCollection = new UnitTestStartup().Services;
//         var (services, _) = serviceCollection.ConfigureTestRepositories();
//
//         var currentInput = inputGenerator.GetValidTlgInputCommandMessage(
//             tlgAgent.Mode,
//             (int)OperationsBotCommands.NewIssue,
//             roleSpecified: currentRole);
//
//         var expectedOutput = "Please select a Trade:";
//         var workflow = services.GetRequiredService<INewIssueWorkflow>();
//
//         var actualOutput =
//             await workflow.GetResponseAsync(currentInput);
//         
//         Assert.Equal(
//             expectedOutput,
//             TestUtils.GetFirstRawEnglish(actualOutput.GetValueOrThrow().Output));
//     }
// }