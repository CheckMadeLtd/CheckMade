using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.UserInteraction;
using CheckMade.Common.Model.Telegram.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Logic.Workflows;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class InputProcessorTests
{
    private ServiceProvider? _services;

    // [Theory]
    // [InlineData(InteractionMode.Operations)]
    // [InlineData(InteractionMode.Communications)]
    // [InlineData(InteractionMode.Notifications)]
    // public async Task ProcessInputAsync_LaunchesUserAuthWorkflow_ForAnyInput_WhenUserNotAuthenticated(
    //     InteractionMode mode)
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     var basics = GetBasicTestingServices(_services, mode);
    //     var unmappedTlgClientPort = new TlgClientPort(2468L, 13563897L);
    //     var input = basics.utils.GetValidTlgTextMessage(
    //         unmappedTlgClientPort.UserId, unmappedTlgClientPort.ChatId);
    //
    //     var outputInUnmappedPort = await basics.processor.ProcessInputAsync(input);
    //     
    //     Assert.Equal(outputInUnmappedPort[0].Text.GetValueOrThrow(), 
    //         UserAuthWorkflow.AuthenticateWithToken);
    // }
    //
    // [Theory]
    // [InlineData(InteractionMode.Operations)]
    // [InlineData(InteractionMode.Communications)]
    // [InlineData(InteractionMode.Notifications)]
    // public async Task ProcessInputAsync_ReturnsRelevantOutput_ForNewIssueBotCommand(InteractionMode mode)
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     var basics = GetBasicTestingServices(_services, mode);
    //     var issueCommandInput = basics.utils.GetValidTlgCommandMessage(
    //         InteractionMode.Operations, (int)OperationsBotCommands.NewIssue);
    //
    //     var actualOutput = await basics.processor.ProcessInputAsync(issueCommandInput);
    //     
    //     Assert.Contains(DomainCategory.SanitaryOps_IssueCleanliness,
    //         actualOutput[0].DomainCategorySelection.GetValueOrThrow());
    // }
    
    private static (ITestUtils utils, IInputProcessor processor) 
        GetBasicTestingServices(IServiceProvider sp, InteractionMode mode) =>
            (sp.GetRequiredService<ITestUtils>(), 
                sp.GetRequiredService<IInputProcessorFactory>().GetInputProcessor(mode));
}
