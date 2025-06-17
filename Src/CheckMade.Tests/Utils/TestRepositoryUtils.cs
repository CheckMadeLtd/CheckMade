using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Data.ChatBot;
using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Abstract.Domain.Data.Core.LiveEvents;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Abstract.Domain.Interfaces.Persistence.Core;
using General.Utils.FpExtensions.Monads;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using User = CheckMade.Abstract.Domain.Data.Core.Actors.User;

namespace CheckMade.Tests.Utils;

internal static class TestRepositoryUtils
{
    internal static AgentRoleBind GetNewRoleBind(
        Role role, 
        Agent agent)
    {
        return new AgentRoleBind(
            role,
            agent,
            DateTimeOffset.UtcNow,
            Option<DateTimeOffset>.None());
    }

    internal static (ServiceProvider sp, MockContainer container) ConfigureTestRepositories(
        this IServiceCollection serviceCollection,
        LiveEvent? liveEvent = null,
        IReadOnlyCollection<LiveEvent>? liveEvents = null,
        IReadOnlyCollection<User>? users = null,
        IReadOnlyCollection<Role>? roles = null,
        IReadOnlyCollection<AgentRoleBind>? roleBindings = null,
        IReadOnlyCollection<Input>? inputs = null,
        IReadOnlyCollection<WorkflowBridge>? bridges = null)
    {
        var defaultLiveEvent = X2024;
        List<LiveEvent> defaultLiveEvents = [X2024, X2025];
        List<User> defaultUsers = [DanielEn, DanielDe];
        List<Role> defaultRoles = [SanitaryAdmin_DanielEn_X2024];
        List<AgentRoleBind> defaultRoleBindings = 
            [GetNewRoleBind(SanitaryAdmin_DanielEn_X2024, PrivateBotChat_Operations)];
        List<Input> defaultInputs = [];
        List<WorkflowBridge> defaultBridges = [];

        var mockContainer = new MockContainer();
        
        serviceCollection = serviceCollection
            .ArrangeTestLiveEventsRepo(liveEvent ?? defaultLiveEvent, liveEvents ?? defaultLiveEvents, mockContainer)
            .ArrangeTestUsersRepo(users ?? defaultUsers, mockContainer)
            .ArrangeTestRolesRepo(roles ?? defaultRoles, mockContainer)
            .ArrangeTestRoleBindingsRepo(roleBindings ?? defaultRoleBindings, mockContainer)
            .ArrangeTestInputsRepo(inputs ?? defaultInputs, mockContainer)
            .ArrangeTestDerivedWorkflowBridgesRepo(bridges ?? defaultBridges, mockContainer);

        return (serviceCollection.BuildServiceProvider(), mockContainer);
    }

    private static IServiceCollection ArrangeTestLiveEventsRepo(
        this IServiceCollection serviceCollection,
        LiveEvent liveEvent,
        IReadOnlyCollection<LiveEvent> liveEvents,
        MockContainer container)
    {
        var mockLiveEventsRepo = new Mock<ILiveEventsRepository>();

        mockLiveEventsRepo
            .Setup(static repo => 
                repo.GetAsync(It.IsAny<ILiveEventInfo>()))
            .ReturnsAsync(liveEvent);
        
        mockLiveEventsRepo
            .Setup(static repo => repo.GetAllAsync())
            .ReturnsAsync(liveEvents);

        container.Mocks[typeof(ILiveEventsRepository)] = mockLiveEventsRepo;
        var stubLiveEventsRepo = mockLiveEventsRepo.Object;
        
        return serviceCollection.AddScoped<ILiveEventsRepository>(_ => stubLiveEventsRepo);
    }
    
    private static IServiceCollection ArrangeTestUsersRepo(
        this IServiceCollection serviceCollection, 
        IReadOnlyCollection<User> users,
        MockContainer container)
    {
        var mockUsersRepo = new Mock<IUsersRepository>();
        mockUsersRepo
            .Setup(static repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        container.Mocks[typeof(IUsersRepository)] = mockUsersRepo;
        var stubUsersRepo = mockUsersRepo.Object;
        
        return serviceCollection.AddScoped<IUsersRepository>(_ => stubUsersRepo);
    }

    private static IServiceCollection ArrangeTestRolesRepo(
        this IServiceCollection serviceCollection, 
        IReadOnlyCollection<Role> roles,
        MockContainer container)
    {
        var mockRolesRepo = new Mock<IRolesRepository>();
        mockRolesRepo
            .Setup(static repo => repo.GetAllAsync())
            .ReturnsAsync(roles);

        container.Mocks[typeof(IRolesRepository)] = mockRolesRepo;
        var stubRolesRepo = mockRolesRepo.Object;
        
        return serviceCollection.AddScoped<IRolesRepository>(_ => stubRolesRepo);
    }

    private static IServiceCollection ArrangeTestRoleBindingsRepo(
        this IServiceCollection serviceCollection, 
        IReadOnlyCollection<AgentRoleBind> roleBindings,
        MockContainer container)
    {
        var mockRoleBindingsRepo = new Mock<IAgentRoleBindingsRepository>();
        
        mockRoleBindingsRepo
            .Setup(static repo => repo.GetAllAsync())
            .ReturnsAsync(roleBindings);
        
        mockRoleBindingsRepo
            .Setup(static repo => repo.GetAllActiveAsync())
            .ReturnsAsync(roleBindings
                .Where(static arb => arb.Status == DbRecordStatus.Active)
                .ToImmutableArray());

        container.Mocks[typeof(IAgentRoleBindingsRepository)] = mockRoleBindingsRepo;
        var stubRoleBindingsRepo = mockRoleBindingsRepo.Object;
        
        return serviceCollection.AddScoped<IAgentRoleBindingsRepository>(_ => stubRoleBindingsRepo);
    }


    private static IServiceCollection ArrangeTestInputsRepo(
        this IServiceCollection serviceCollection,
        IReadOnlyCollection<Input> inputs,
        MockContainer container)
    {
        var mockInputsRepo = new Mock<IInputsRepository>();
        
        mockInputsRepo
            .Setup(static repo => 
                repo.GetAllInteractiveAsync(It.IsAny<Agent>()))
            .ReturnsAsync((Agent agent) => 
                inputs
                    .Where(i => 
                        i.Agent.Equals(agent) &&
                        i.InputType != InputType.Location)
                    .ToImmutableArray());
        
        mockInputsRepo
            .Setup(static repo => 
                repo.GetAllInteractiveAsync(It.IsAny<ILiveEventInfo>()))
            .ReturnsAsync((ILiveEventInfo liveEvent) => 
                inputs
                    .Where(i => 
                        Equals(i.LiveEventContext.GetValueOrDefault(), liveEvent) &&
                        i.InputType != InputType.Location)
                    .ToImmutableArray());

        mockInputsRepo
            .Setup(static repo =>
                repo.GetAllLocationAsync(
                    It.IsAny<Agent>(),
                    It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((Agent agent, DateTimeOffset dateTime) =>
                inputs
                    .Where(i => 
                        i.Agent.Equals(agent) && 
                        i.TimeStamp >= dateTime &&
                        i.InputType == InputType.Location)
                    .ToImmutableArray());

        mockInputsRepo
            .Setup(static repo =>
                repo.GetEntityHistoryAsync(
                    It.IsAny<ILiveEventInfo>(),
                    It.IsAny<Guid>()))
            .ReturnsAsync((ILiveEventInfo liveEvent, Guid entityGuid) =>
                inputs
                    .Where(i =>
                        Equals(i.LiveEventContext.GetValueOrDefault(), liveEvent) &&
                        Equals(i.EntityGuid.GetValueOrDefault(), entityGuid))
                    .ToImmutableArray());
        
        container.Mocks[typeof(IInputsRepository)] = mockInputsRepo;
        var stubInputsRepo = mockInputsRepo.Object;
        
        return serviceCollection.AddScoped<IInputsRepository>(_ => stubInputsRepo);
    }

    private static IServiceCollection ArrangeTestDerivedWorkflowBridgesRepo(
        this IServiceCollection serviceCollection,
        IReadOnlyCollection<WorkflowBridge> bridges,
        MockContainer container)
    {
        var mockBridgesRepo = new Mock<IDerivedWorkflowBridgesRepository>();

        mockBridgesRepo
            .Setup(static repo =>
                repo.GetAllAsync(It.IsAny<ILiveEventInfo>()))
            .ReturnsAsync((ILiveEventInfo liveEvent) =>
                bridges
                    .Where(b =>
                        Equals(b.SourceInput.LiveEventContext.GetValueOrDefault(), liveEvent))
                    .ToImmutableArray());
        
        // ToDo: Implement also repo.GetAsync() if/when needed for testing? If not, remove ToDo!

        container.Mocks[typeof(IDerivedWorkflowBridgesRepository)] = mockBridgesRepo;
        var stubBridgesRepo = mockBridgesRepo.Object;

        return serviceCollection.AddScoped<IDerivedWorkflowBridgesRepository>(_ => stubBridgesRepo);
    }

    internal sealed record MockContainer
    {
        internal Dictionary<Type, object> Mocks { get; } = new();
    }
}