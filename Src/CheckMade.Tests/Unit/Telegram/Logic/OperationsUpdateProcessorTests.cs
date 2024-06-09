using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Logic.UpdateProcessors;
using CheckMade.Telegram.Logic.UpdateProcessors.Concrete;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Telegram.Logic;

public class OperationsUpdateProcessorTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task ProcessUpdateAsync_PromptsAuth_ForAnyInputExceptStartCommand_WhenTelegramPortNotMappedToRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var unmappedTelegramPort = new TelegramPort(2468L, 13563897L);
        var update = basics.utils.GetValidModelTextMessage(
            unmappedTelegramPort.UserId, unmappedTelegramPort.ChatId);
    
        var outputInUnMappedChatId = await basics.processor.ProcessUpdateAsync(update);
        
        Assert.Equal(outputInUnMappedChatId[0].Text.GetValueOrThrow(), 
            IUpdateProcessor.AuthenticateWithToken);
    }
    
    [Fact]
    public async Task ProcessUpdateAsync_ReturnsRelevantOutput_ForNewIssueBotCommand()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var issueCommandUpdate = basics.utils.GetValidModelCommandMessage(
            BotType.Operations, (int)OperationsBotCommands.NewIssue);

        var actualOutput = await basics.processor.ProcessUpdateAsync(issueCommandUpdate);
        
        Assert.Contains(DomainCategory.SanitaryOps_IssueCleanliness,
            actualOutput[0].DomainCategorySelection.GetValueOrThrow());
    }
    
    private static (ITestUtils utils, IOperationsUpdateProcessor processor) 
        GetBasicTestingServices(IServiceProvider sp) =>
            (sp.GetRequiredService<ITestUtils>(), sp.GetRequiredService<IOperationsUpdateProcessor>());
}