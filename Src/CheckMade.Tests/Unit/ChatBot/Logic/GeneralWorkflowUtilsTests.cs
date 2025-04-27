using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public sealed class GeneralWorkflowUtilsTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsAllInputs_WhenNoExpiredRoleBinds()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        
        var historicInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId)
        };
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();
        
        var currentInput = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgent.UserId, tlgAgent.ChatId);

        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(tlgAgent, currentInput);

        Assert.Equal(
            historicInputs.Length + 1,
            result.Count);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsInputsAfterCutoffDate_WhenExpiredRoleBindExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-1);
        var expiredRoleBind = new TlgAgentRoleBind(
            SanitaryAdmin_DanielEn_X2024,
            tlgAgent,
            cutoffDate.AddDays(-2),
            cutoffDate,
            DbRecordStatus.Historic);

        var historicInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId, dateTime: cutoffDate.AddHours(-1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId, dateTime: cutoffDate.AddHours(1))
        };
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: [expiredRoleBind],
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgent.UserId, tlgAgent.ChatId, dateTime: cutoffDate.AddHours(2));

        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(tlgAgent, currentInput);

        Assert.Equal(
            2,
            result.Count);
        Assert.All(
            result,
            input => Assert.True(
                input.TlgDate > cutoffDate));
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsInputsAfterLatestExpiredRoleBind_WhenMultipleExpiredExist()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var oldestCutoffDate = DateTimeOffset.UtcNow.AddDays(-3);
        var latestCutoffDate = DateTimeOffset.UtcNow.AddDays(-1);

        var expiredRoleBinds = new[]
        {
            new TlgAgentRoleBind(
                SanitaryAdmin_DanielEn_X2024,
                tlgAgent,
                oldestCutoffDate.AddDays(-1),
                oldestCutoffDate,
                DbRecordStatus.Historic),
            
            new TlgAgentRoleBind(
                SanitaryInspector_DanielEn_X2024,
                tlgAgent,
                latestCutoffDate.AddDays(-1),
                latestCutoffDate,
                DbRecordStatus.Historic)
        };

        var historicInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: oldestCutoffDate.AddHours(-1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: oldestCutoffDate.AddHours(1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: latestCutoffDate.AddHours(-1))
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: expiredRoleBinds,
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgent.UserId, tlgAgent.ChatId,
            dateTime: latestCutoffDate.AddHours(1));
        
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(tlgAgent, currentInput);

        Assert.Single(result);
        Assert.True(
            result.Single().TlgDate > latestCutoffDate);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsEmptyCollection_WhenNoInputsAfterLatestExpiredRoleBind()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-1);
        var expiredRoleBind = new TlgAgentRoleBind(
            SanitaryAdmin_DanielEn_X2024,
            tlgAgent,
            cutoffDate.AddDays(-2),
            cutoffDate,
            DbRecordStatus.Historic);

        var historicInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: cutoffDate.AddHours(-2))
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: [expiredRoleBind],
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgent.UserId, tlgAgent.ChatId,
            dateTime: cutoffDate.AddHours(-1));
        
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(tlgAgent, currentInput);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_HandlesNullDeactivationDate_InExpiredRoleBinds()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var roleBindWithNullDeactivation = new TlgAgentRoleBind(
            SanitaryAdmin_DanielEn_X2024,
            tlgAgent,
            DateTimeOffset.UtcNow.AddDays(-2),
            Option<DateTimeOffset>.None(),
            DbRecordStatus.Historic);

        var historicInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId)
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: [roleBindWithNullDeactivation],
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgent.UserId, tlgAgent.ChatId);
            
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(tlgAgent, currentInput);

        Assert.Equal(
            historicInputs.Length + 1,
            result.Count);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_FiltersInputsBySpecificTlgAgent()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var tlgAgentDecoy = UserId02_ChatId03_Operations;

        var historicInputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgentDecoy.UserId, tlgAgentDecoy.ChatId)
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidTlgInputTextMessage(
            tlgAgent.UserId, tlgAgent.ChatId);
            
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(tlgAgent, currentInput);
        
        Assert.Equal(
            2, result.Count);
        Assert.All(
            result,
            input => Assert.Equal(tlgAgent.UserId, input.TlgAgent.UserId));
        Assert.All(
            result,
            input => Assert.Equal(tlgAgent.ChatId, input.TlgAgent.ChatId));
    }

    [Fact]
    public async Task GetRecentLocationHistory_FiltersInputs_ByTlgAgentAndLocationTypeAndTimeFrame()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;
        var tlgAgentDecoy = UserId02_ChatId03_Operations;

        var randomDecoyLocation = 
            new Geo(0, 0, Option<double>.None());

        var expectedLocation =
            new Geo(1, 1, Option<double>.None());

        var historicInputs = new[]
        {
            // Decoy: too long ago
            inputGenerator.GetValidTlgInputLocationMessage(
                randomDecoyLocation,
                tlgAgent.UserId, tlgAgent.ChatId,
                DateTimeOffset.UtcNow.AddMinutes(-(IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes + 2))),
            // Decoy: wrong TlgAgent
            inputGenerator.GetValidTlgInputLocationMessage(
                randomDecoyLocation,
                tlgAgentDecoy.UserId, tlgAgentDecoy.ChatId,
                DateTimeOffset.UtcNow.AddMinutes(-(IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes - 1))),
            // Decoy: not a LocationUpdate
            inputGenerator.GetValidTlgInputTextMessage(),
            
            // Expected to be included
            inputGenerator.GetValidTlgInputLocationMessage(
                expectedLocation,
                tlgAgent.UserId, tlgAgent.ChatId,
                DateTimeOffset.UtcNow.AddMinutes(-(IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes - 1)))
        };
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();
        
        var result = 
            await workflowUtils.GetRecentLocationHistory(tlgAgent);
        
        Assert.Single(result);
        Assert.Equivalent(
            expectedLocation, 
            result.First().Details.GeoCoordinates.GetValueOrThrow());
    }
}