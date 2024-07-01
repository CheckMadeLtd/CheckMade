using CheckMade.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Unit.ChatBot.Logic;

public class LogicUtilsTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetAllCurrentInputsAsync_ReturnsAllInputs_WhenNoExpiredRoleBinds()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var inputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId)
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: inputs);
        _services = services;

        var logicUtils = _services.GetRequiredService<ILogicUtils>();
        
        var result = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent);

        Assert.Equal(
            inputs.Length,
            result.Count);
    }

    [Fact]
    public async Task GetAllCurrentInputsAsync_ReturnsInputsAfterCutoffDate_WhenExpiredRoleBindExists()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var cutoffDate = DateTime.UtcNow.AddDays(-1);
        var expiredRoleBind = new TlgAgentRoleBind(
            SOpsAdmin_DanielEn_X2024,
            tlgAgent,
            cutoffDate.AddDays(-2),
            cutoffDate,
            DbRecordStatus.Historic);

        var inputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId, dateTime: cutoffDate.AddHours(-1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId, dateTime: cutoffDate.AddHours(1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId, dateTime: cutoffDate.AddHours(2))
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: new[] { expiredRoleBind },
            inputs: inputs);
        _services = services;

        var logicUtils = _services.GetRequiredService<ILogicUtils>();

        var result = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent);

        Assert.Equal(
            2,
            result.Count);
        Assert.All(
            result,
            input => Assert.True(
                input.Details.TlgDate > cutoffDate));
    }

    [Fact]
    public async Task GetAllCurrentInputsAsync_ReturnsInputsAfterLatestExpiredRoleBind_WhenMultipleExpiredExist()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var oldestCutoffDate = DateTime.UtcNow.AddDays(-3);
        var latestCutoffDate = DateTime.UtcNow.AddDays(-1);

        var expiredRoleBinds = new[]
        {
            new TlgAgentRoleBind(
                SOpsAdmin_DanielEn_X2024,
                tlgAgent,
                oldestCutoffDate.AddDays(-1),
                oldestCutoffDate,
                DbRecordStatus.Historic),
            
            new TlgAgentRoleBind(
                SOpsInspector_DanielEn_X2024,
                tlgAgent,
                latestCutoffDate.AddDays(-1),
                latestCutoffDate,
                DbRecordStatus.Historic)
        };

        var inputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: oldestCutoffDate.AddHours(-1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: oldestCutoffDate.AddHours(1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: latestCutoffDate.AddHours(-1)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: latestCutoffDate.AddHours(1))
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: expiredRoleBinds,
            inputs: inputs);
        _services = services;

        var logicUtils = _services.GetRequiredService<ILogicUtils>();

        var result = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent);

        Assert.Single(result);
        Assert.True(
            result.Single().Details.TlgDate > latestCutoffDate);
    }

    [Fact]
    public async Task GetAllCurrentInputsAsync_ReturnsEmptyCollection_WhenNoInputsAfterLatestExpiredRoleBind()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var cutoffDate = DateTime.UtcNow.AddDays(-1);
        var expiredRoleBind = new TlgAgentRoleBind(
            SOpsAdmin_DanielEn_X2024,
            tlgAgent,
            cutoffDate.AddDays(-2),
            cutoffDate,
            DbRecordStatus.Historic);

        var inputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: cutoffDate.AddHours(-2)),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId,
                dateTime: cutoffDate.AddHours(-1))
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: new[] { expiredRoleBind },
            inputs: inputs);
        _services = services;

        var logicUtils = _services.GetRequiredService<ILogicUtils>();

        var result = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllCurrentInputsAsync_HandlesNullDeactivationDate_InExpiredRoleBinds()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent = PrivateBotChat_Operations;

        var roleBindWithNullDeactivation = new TlgAgentRoleBind(
            SOpsAdmin_DanielEn_X2024,
            tlgAgent,
            DateTime.UtcNow.AddDays(-2),
            Option<DateTime>.None(),
            DbRecordStatus.Historic);

        var inputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent.UserId, tlgAgent.ChatId)
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: new[] { roleBindWithNullDeactivation },
            inputs: inputs);
        _services = services;

        var logicUtils = _services.GetRequiredService<ILogicUtils>();

        var result = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent);

        Assert.Equal(
            inputs.Length,
            result.Count);
    }

    [Fact]
    public async Task GetAllCurrentInputsAsync_FiltersInputsBySpecificTlgAgent()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = _services.GetRequiredService<ITlgInputGenerator>();
        var tlgAgent1 = PrivateBotChat_Operations;
        var tlgAgent2 = UserId02_ChatId03_Operations;

        var inputs = new[]
        {
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent1.UserId, tlgAgent1.ChatId),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent2.UserId, tlgAgent2.ChatId),
            inputGenerator.GetValidTlgInputTextMessage(
                tlgAgent1.UserId, tlgAgent1.ChatId)
        };

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(inputs: inputs);
        _services = services;

        var logicUtils = _services.GetRequiredService<ILogicUtils>();

        var result = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent1);
        
        Assert.Equal(
            2, result.Count);
        Assert.All(
            result,
            input => Assert.Equal(tlgAgent1.UserId, input.TlgAgent.UserId));
        Assert.All(
            result,
            input => Assert.Equal(tlgAgent1.ChatId, input.TlgAgent.ChatId));
    }
    
    [Fact]
    public async Task GetAllCurrentInputsAsync_HandlesNoInputs()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var tlgAgent = PrivateBotChat_Operations;

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            inputs: Array.Empty<TlgInput>());
        _services = services;

        var logicUtils = _services.GetRequiredService<ILogicUtils>();

        var result = 
            await logicUtils.GetAllCurrentInputsAsync(tlgAgent);

        Assert.Empty(result);
    }
}