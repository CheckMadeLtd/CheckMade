using CheckMade.Abstract.Domain.Data.Bot;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.GIS;
using CheckMade.Bot.Workflows.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using General.Utils.FpExtensions.Monads;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.Bot.Workflows;

public sealed class GeneralWorkflowUtilsTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsAllInputs_WhenNoExpiredRoleBinds()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        
        var historicInputs = new[]
        {
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId)
        };
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();
        
        var currentInput = inputGenerator.GetValidInputTextMessage(
            agent.UserId, agent.ChatId);

        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(agent, currentInput);

        Assert.Equal(
            historicInputs.Length + 1,
            result.Count);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsInputsAfterCutoffDate_WhenExpiredRoleBindExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-1);
        var expiredRoleBind = new AgentRoleBind(
            SanitaryAdmin_DanielEn_X2024,
            agent,
            cutoffDate.AddDays(-2),
            cutoffDate,
            DbRecordStatus.Historic);

        var historicInputs = new[]
        {
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId, dateTime: cutoffDate.AddHours(-1)),
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId, dateTime: cutoffDate.AddHours(1))
        };
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: [expiredRoleBind],
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidInputTextMessage(
            agent.UserId, agent.ChatId, dateTime: cutoffDate.AddHours(2));

        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(agent, currentInput);

        Assert.Equal(
            2,
            result.Count);
        Assert.All(
            result,
            input => Assert.True(
                input.TimeStamp > cutoffDate));
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsInputsAfterLatestExpiredRoleBind_WhenMultipleExpiredExist()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;

        var oldestCutoffDate = DateTimeOffset.UtcNow.AddDays(-3);
        var latestCutoffDate = DateTimeOffset.UtcNow.AddDays(-1);

        var expiredRoleBinds = new[]
        {
            new AgentRoleBind(
                SanitaryAdmin_DanielEn_X2024,
                agent,
                oldestCutoffDate.AddDays(-1),
                oldestCutoffDate,
                DbRecordStatus.Historic),
            
            new AgentRoleBind(
                SanitaryInspector_DanielEn_X2024,
                agent,
                latestCutoffDate.AddDays(-1),
                latestCutoffDate,
                DbRecordStatus.Historic)
        };

        var historicInputs = new[]
        {
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId,
                dateTime: oldestCutoffDate.AddHours(-1)),
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId,
                dateTime: oldestCutoffDate.AddHours(1)),
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId,
                dateTime: latestCutoffDate.AddHours(-1))
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: expiredRoleBinds,
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidInputTextMessage(
            agent.UserId, agent.ChatId,
            dateTime: latestCutoffDate.AddHours(1));
        
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(agent, currentInput);

        Assert.Single(result);
        Assert.True(
            result.Single().TimeStamp > latestCutoffDate);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_ReturnsEmptyCollection_WhenNoInputsAfterLatestExpiredRoleBind()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-1);
        var expiredRoleBind = new AgentRoleBind(
            SanitaryAdmin_DanielEn_X2024,
            agent,
            cutoffDate.AddDays(-2),
            cutoffDate,
            DbRecordStatus.Historic);

        var historicInputs = new[]
        {
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId,
                dateTime: cutoffDate.AddHours(-2))
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: [expiredRoleBind],
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidInputTextMessage(
            agent.UserId, agent.ChatId,
            dateTime: cutoffDate.AddHours(-1));
        
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(agent, currentInput);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_HandlesNullDeactivationDate_InExpiredRoleBinds()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;

        var roleBindWithNullDeactivation = new AgentRoleBind(
            SanitaryAdmin_DanielEn_X2024,
            agent,
            DateTimeOffset.UtcNow.AddDays(-2),
            Option<DateTimeOffset>.None(),
            DbRecordStatus.Historic);

        var historicInputs = new[]
        {
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId)
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: [roleBindWithNullDeactivation],
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidInputTextMessage(
            agent.UserId, agent.ChatId);
            
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(agent, currentInput);

        Assert.Equal(
            historicInputs.Length + 1,
            result.Count);
    }

    [Fact]
    public async Task GetAllCurrentInteractiveAsync_FiltersInputsBySpecificAgent()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        var agentDecoy = UserId02_ChatId03_Operations;

        var historicInputs = new[]
        {
            inputGenerator.GetValidInputTextMessage(
                agent.UserId, agent.ChatId),
            inputGenerator.GetValidInputTextMessage(
                agentDecoy.UserId, agentDecoy.ChatId)
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();

        var currentInput = inputGenerator.GetValidInputTextMessage(
            agent.UserId, agent.ChatId);
            
        var result = 
            await workflowUtils.GetAllCurrentInteractiveAsync(agent, currentInput);
        
        Assert.Equal(
            2, result.Count);
        Assert.All(
            result,
            input => Assert.Equal(agent.UserId, input.Agent.UserId));
        Assert.All(
            result,
            input => Assert.Equal(agent.ChatId, input.Agent.ChatId));
    }

    [Fact]
    public async Task GetRecentLocationHistory_FiltersInputs_ByAgentAndLocationTypeAndTimeFrame()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var inputGenerator = _services.GetRequiredService<IInputGenerator>();
        var agent = PrivateBotChat_Operations;
        var agentDecoy = UserId02_ChatId03_Operations;

        var randomDecoyLocation = 
            new Geo(0, 0, Option<double>.None());

        var expectedLocation =
            new Geo(1, 1, Option<double>.None());

        var historicInputs = new[]
        {
            // Decoy: too long ago
            inputGenerator.GetValidInputLocationMessage(
                randomDecoyLocation,
                agent.UserId, agent.ChatId,
                DateTimeOffset.UtcNow.AddMinutes(-(IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes + 2))),
            // Decoy: wrong Agent
            inputGenerator.GetValidInputLocationMessage(
                randomDecoyLocation,
                agentDecoy.UserId, agentDecoy.ChatId,
                DateTimeOffset.UtcNow.AddMinutes(-(IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes - 1))),
            // Decoy: not a LocationUpdate
            inputGenerator.GetValidInputTextMessage(),
            
            // Expected to be included
            inputGenerator.GetValidInputLocationMessage(
                expectedLocation,
                agent.UserId, agent.ChatId,
                DateTimeOffset.UtcNow.AddMinutes(-(IGeneralWorkflowUtils.RecentLocationHistoryTimeFrameInMinutes - 1)))
        };
        
        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: historicInputs);
        var workflowUtils = services.GetRequiredService<IGeneralWorkflowUtils>();
        
        var result = 
            await workflowUtils.GetRecentLocationHistory(agent);
        
        Assert.Single(result);
        Assert.Equivalent(
            expectedLocation, 
            result.First().Details.GeoCoordinates.GetValueOrThrow());
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsElementsFromTheEndUpToStopConditionIncluding_WhenInclusive()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = static x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);
        
        Assert.Equal([3, 4, 5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsElementsFromTheEndUpToStopConditionExcluding_WhenNotInclusive()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = static x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, false);
    
        Assert.Equal([4, 5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsAllElements_WhenStopConditionItemNotPresent()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = static x => x == 7;
        var result = list.GetLatestRecordsUpTo(stopCondition, false);
    
        Assert.Equal(list, result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsNoElements_WhenStopConditionItemIsLastOne_Excluding()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = static x => x == 5;
        var result = list.GetLatestRecordsUpTo(stopCondition, false);
    
        Assert.Equal([], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsLastElement_WhenStopConditionItemIsLastOne_Including()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = static x => x == 5;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);
    
        Assert.Equal([5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsOnlyElement_WhenItIsStopConditionItem_Including()
    {
        List<int> list = [5];
        Func<int, bool> stopCondition = static x => x == 5;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);
    
        Assert.Equal([5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsNoElements_WhenInputListIsEmpty()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<int> list = [];
        Func<int, bool> stopCondition = static x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);

        Assert.Equal(list, result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsCorrectElements_WhenMultipleStopConditionsPresent()
    {
        List<int> list = [1, 3, 2, 3, 4, 5];
        Func<int, bool> stopCondition = static x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);

        Assert.Equal([3, 4, 5], result);
    }
}