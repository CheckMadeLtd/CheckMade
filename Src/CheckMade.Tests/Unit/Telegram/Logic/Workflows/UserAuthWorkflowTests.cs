// using CheckMade.Telegram.Logic.Workflows;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace CheckMade.Tests.Unit.Telegram.Logic.Workflows;
//
// public class UserAuthWorkflowTests
// {
//     private ServiceProvider? _services;
//
//     /*
//      * Passing this test will require us to change GetNextOutput so that it distinguishes between the case of
//      * being in that workflow for the first time (in which case no such feedback is given to any value for tlgInput)
//      * vs when the user has pressed the corresp. ControlPrompt and then enters the token.
//      * To test this we need to set up a mocked tlg_inputs repo that has the input history required for the test case.
//      *
//      * But first, we need to change the current test so that it shows the correct message with the ControlPrompt.
//      */
//     
//     [Fact]
//     public void GetNextOutputAsync_ReturnsFailedResultWithUsefulErrorMessage_WhenFormatOfEnteredTokenIsInvalid()
//     {
//         var workflow = new UserAuthWorkflow();
//         
//     }
// }