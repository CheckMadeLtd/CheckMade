using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Logic.UpdateProcessors.Concrete;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class OperationsUpdateProcessorTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task ProcessUpdateAsync_ReturnsRelevantOutput_ForNewIssueBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var problemCommandUpdate = basics.utils.GetValidModelInputCommandMessage(
            BotType.Operations, (int)OperationsBotCommands.NewIssue);

        var actualOutput = await basics.processor.ProcessUpdateAsync(problemCommandUpdate);
        
        Assert.Contains(DomainCategory.SanitaryOps_IssueCleanliness,
            actualOutput[0].DomainCategorySelection.GetValueOrDefault());
    }
    
    private static (ITestUtils utils, IOperationsUpdateProcessor processor) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(), sp.GetRequiredService<IOperationsUpdateProcessor>());
}