using CheckMade.Common.Model.Tlg;
using CheckMade.Common.Model.UserInteraction;
using CheckMade.Telegram.Logic.InputProcessors;
using CheckMade.Telegram.Logic.InputProcessors.Concrete;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByInteractionMode;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class OperationsInputProcessorTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task ProcessInputAsync_PromptsAuth_ForAnyInputExceptStartCommand_WhenTlgClientPortNotMappedToRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var unmappedTlgClientPort = new TlgClientPort(2468L, 13563897L);
        var input = basics.utils.GetValidTlgTextMessage(
            unmappedTlgClientPort.UserId, unmappedTlgClientPort.ChatId);
    
        var outputInUnmappedPort = await basics.processor.ProcessInputAsync(input);
        
        Assert.Equal(outputInUnmappedPort[0].Text.GetValueOrThrow(), 
            IInputProcessor.AuthenticateWithToken);
    }
    
    [Fact]
    public async Task ProcessInputAsync_ReturnsRelevantOutput_ForNewIssueBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var issueCommandInput = basics.utils.GetValidTlgCommandMessage(
            InteractionMode.Operations, (int)OperationsBotCommands.NewIssue);

        var actualOutput = await basics.processor.ProcessInputAsync(issueCommandInput);
        
        Assert.Contains(DomainCategory.SanitaryOps_IssueCleanliness,
            actualOutput[0].DomainCategorySelection.GetValueOrThrow());
    }
    
    private static (ITestUtils utils, IOperationsInputProcessor processor) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(), sp.GetRequiredService<IOperationsInputProcessor>());
}