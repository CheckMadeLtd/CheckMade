using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.TelegramUpdates;
using CheckMade.Telegram.Logic.RequestProcessors.Concrete;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class OperationsRequestProcessorTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task ProcessRequestAsync_ReturnsRelevantOutput_ForNewIssueBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var problemCommandUpdate = basics.utils.GetValidModelInputCommandMessage(
            BotType.Operations, (int)OperationsBotCommands.NewIssue);

        var actualOutput = await basics.processor.ProcessRequestAsync(problemCommandUpdate);
        
        Assert.True(actualOutput.IsSuccess);
        Assert.Contains(DomainCategory.SanitaryOps_IssueCleanliness,
            actualOutput.GetValueOrDefault()[0].DomainCategorySelection.GetValueOrDefault());
    }
    
    private static (ITestUtils utils, IOperationsRequestProcessor processor) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(), sp.GetRequiredService<IOperationsRequestProcessor>());
}