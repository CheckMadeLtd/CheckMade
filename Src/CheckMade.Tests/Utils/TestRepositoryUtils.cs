using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using User = CheckMade.Common.Model.Core.Actors.Concrete.User;

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
        IReadOnlyCollection<TlgInput>? inputs = null)
    {
        var defaultLiveEvent = X2024;
        List<LiveEvent> defaultLiveEvents = [X2024, X2025];
        List<User> defaultUsers = [DanielEn, DanielDe];
        List<Role> defaultRoles = [SaniCleanAdmin_DanielEn_X2024];
        List<TlgAgentRoleBind> defaultRoleBindings = 
            [GetNewRoleBind(SaniCleanAdmin_DanielEn_X2024, PrivateBotChat_Operations)];
        List<TlgInput> defaultInputs = [];

        var mockContainer = new MockContainer();
        
        serviceCollection = serviceCollection
            .ArrangeTestLiveEventsRepo(liveEvent ?? defaultLiveEvent, liveEvents ?? defaultLiveEvents, mockContainer)
            .ArrangeTestUsersRepo(users ?? defaultUsers, mockContainer)
            .ArrangeTestRolesRepo(roles ?? defaultRoles, mockContainer)
            .ArrangeTestRoleBindingsRepo(roleBindings ?? defaultRoleBindings, mockContainer)
            .ArrangeTestTlgInputsRepo(inputs ?? defaultInputs, mockContainer);

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
            .Setup(repo => 
                repo.GetAsync(It.IsAny<ILiveEventInfo>()))
            .ReturnsAsync(liveEvent);
        
        mockLiveEventsRepo
            .Setup(repo => repo.GetAllAsync())
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
            .Setup(repo => repo.GetAllAsync())
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
            .Setup(repo => repo.GetAllAsync())
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
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(roleBindings);
        
        mockRoleBindingsRepo
            .Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(roleBindings
                .Where(tarb => tarb.Status == DbRecordStatus.Active)
                .ToImmutableReadOnlyCollection);

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
            .Setup(repo => 
                repo.GetAllInteractiveAsync(It.IsAny<TlgAgent>()))
            .ReturnsAsync((TlgAgent tlgAgent) => 
                inputs
                    .Where(i => 
                        i.TlgAgent.Equals(tlgAgent) &&
                        i.InputType != TlgInputType.Location)
                    .ToImmutableReadOnlyCollection());
        
        mockTlgInputsRepo
            .Setup(repo => 
                repo.GetAllInteractiveAsync(It.IsAny<ILiveEventInfo>()))
            .ReturnsAsync((ILiveEventInfo liveEvent) => 
                inputs
                    .Where(i => 
                        Equals(i.LiveEventContext.GetValueOrDefault(), liveEvent) &&
                        i.InputType != TlgInputType.Location)
                    .ToImmutableReadOnlyCollection());

        mockTlgInputsRepo
            .Setup(repo =>
                repo.GetAllLocationAsync(
                    It.IsAny<TlgAgent>(),
                    It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((TlgAgent tlgAgent, DateTimeOffset dateTime) =>
                inputs
                    .Where(i => 
                        i.TlgAgent.Equals(tlgAgent) && 
                        i.TlgDate >= dateTime &&
                        i.InputType == TlgInputType.Location)
                    .ToImmutableReadOnlyCollection());
        
        container.Mocks[typeof(ITlgInputsRepository)] = mockTlgInputsRepo;
        var stubTlgInputsRepo = mockTlgInputsRepo.Object;
        
        return serviceCollection.AddScoped<ITlgInputsRepository>(_ => stubTlgInputsRepo);
    }

    internal sealed record MockContainer
    {
        internal Dictionary<Type, object> Mocks { get; } = new();
    }
}