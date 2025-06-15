using System.Collections.Immutable;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Common.Domain.Data.Core.LiveEvents;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using User = CheckMade.Common.Domain.Data.Core.Actors.User;

namespace CheckMade.Tests.Utils;

internal static class TestRepositoryUtils
{
    internal static TlgAgentRoleBind GetNewRoleBind(
        Role role, 
        TlgAgent tlgAgent)
    {
        return new TlgAgentRoleBind(
            role,
            tlgAgent,
            DateTimeOffset.UtcNow,
            Option<DateTimeOffset>.None());
    }

    internal static (ServiceProvider sp, MockContainer container) ConfigureTestRepositories(
        this IServiceCollection serviceCollection,
        LiveEvent? liveEvent = null,
        IReadOnlyCollection<LiveEvent>? liveEvents = null,
        IReadOnlyCollection<User>? users = null,
        IReadOnlyCollection<Role>? roles = null,
        IReadOnlyCollection<TlgAgentRoleBind>? roleBindings = null,
        IReadOnlyCollection<TlgInput>? inputs = null,
        IReadOnlyCollection<WorkflowBridge>? bridges = null)
    {
        var defaultLiveEvent = X2024;
        List<LiveEvent> defaultLiveEvents = [X2024, X2025];
        List<User> defaultUsers = [DanielEn, DanielDe];
        List<Role> defaultRoles = [SanitaryAdmin_DanielEn_X2024];
        List<TlgAgentRoleBind> defaultRoleBindings = 
            [GetNewRoleBind(SanitaryAdmin_DanielEn_X2024, PrivateBotChat_Operations)];
        List<TlgInput> defaultInputs = [];
        List<WorkflowBridge> defaultBridges = [];

        var mockContainer = new MockContainer();
        
        serviceCollection = serviceCollection
            .ArrangeTestLiveEventsRepo(liveEvent ?? defaultLiveEvent, liveEvents ?? defaultLiveEvents, mockContainer)
            .ArrangeTestUsersRepo(users ?? defaultUsers, mockContainer)
            .ArrangeTestRolesRepo(roles ?? defaultRoles, mockContainer)
            .ArrangeTestRoleBindingsRepo(roleBindings ?? defaultRoleBindings, mockContainer)
            .ArrangeTestTlgInputsRepo(inputs ?? defaultInputs, mockContainer)
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
        IReadOnlyCollection<TlgAgentRoleBind> roleBindings,
        MockContainer container)
    {
        var mockRoleBindingsRepo = new Mock<ITlgAgentRoleBindingsRepository>();
        
        mockRoleBindingsRepo
            .Setup(static repo => repo.GetAllAsync())
            .ReturnsAsync(roleBindings);
        
        mockRoleBindingsRepo
            .Setup(static repo => repo.GetAllActiveAsync())
            .ReturnsAsync(roleBindings
                .Where(static tarb => tarb.Status == DbRecordStatus.Active)
                .ToImmutableArray());

        container.Mocks[typeof(ITlgAgentRoleBindingsRepository)] = mockRoleBindingsRepo;
        var stubRoleBindingsRepo = mockRoleBindingsRepo.Object;
        
        return serviceCollection.AddScoped<ITlgAgentRoleBindingsRepository>(_ => stubRoleBindingsRepo);
    }


    private static IServiceCollection ArrangeTestTlgInputsRepo(
        this IServiceCollection serviceCollection,
        IReadOnlyCollection<TlgInput> inputs,
        MockContainer container)
    {
        var mockTlgInputsRepo = new Mock<ITlgInputsRepository>();
        
        mockTlgInputsRepo
            .Setup(static repo => 
                repo.GetAllInteractiveAsync(It.IsAny<TlgAgent>()))
            .ReturnsAsync((TlgAgent tlgAgent) => 
                inputs
                    .Where(i => 
                        i.TlgAgent.Equals(tlgAgent) &&
                        i.InputType != InputType.Location)
                    .ToImmutableArray());
        
        mockTlgInputsRepo
            .Setup(static repo => 
                repo.GetAllInteractiveAsync(It.IsAny<ILiveEventInfo>()))
            .ReturnsAsync((ILiveEventInfo liveEvent) => 
                inputs
                    .Where(i => 
                        Equals(i.LiveEventContext.GetValueOrDefault(), liveEvent) &&
                        i.InputType != InputType.Location)
                    .ToImmutableArray());

        mockTlgInputsRepo
            .Setup(static repo =>
                repo.GetAllLocationAsync(
                    It.IsAny<TlgAgent>(),
                    It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((TlgAgent tlgAgent, DateTimeOffset dateTime) =>
                inputs
                    .Where(i => 
                        i.TlgAgent.Equals(tlgAgent) && 
                        i.TlgDate >= dateTime &&
                        i.InputType == InputType.Location)
                    .ToImmutableArray());

        mockTlgInputsRepo
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
        
        container.Mocks[typeof(ITlgInputsRepository)] = mockTlgInputsRepo;
        var stubTlgInputsRepo = mockTlgInputsRepo.Object;
        
        return serviceCollection.AddScoped<ITlgInputsRepository>(_ => stubTlgInputsRepo);
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