using CheckMade.Common.Model.Enums;
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
    public async Task ProcessUpdateAsync_AsksForTokenForAnyInputOtherThanStart_WhenUserChatNotMapped()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const long userId = 2468L;
        const long unmappedChatId = 13563897L; // random choice
        
        var updateInUnmappedChatId = basics.utils.GetValidModelTextMessage(userId, unmappedChatId);
    
        var outputInUnMappedChatId = 
            await basics.processor.ProcessUpdateAsync(updateInUnmappedChatId);
        
        Assert.Equal(outputInUnMappedChatId[0].Text.GetValueOrThrow(), IUpdateProcessor.AuthenticateWithToken);
    }
    
    // Test: every bot should ask for token etc. this is shared behaviour
    
    // Test: adapt test so it actually checks for 'current' mapping, i.e. chatIdToDestination mapping need to have status, 
    // historic ones are preserved, but if mapping is non-current, it asks for a new auth. 
    
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