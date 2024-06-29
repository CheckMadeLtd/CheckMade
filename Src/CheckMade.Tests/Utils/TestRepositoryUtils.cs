using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using User = CheckMade.Common.Model.Core.User;

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
            DateTime.UtcNow,
            Option<DateTime>.None());
    }

    internal static (ServiceProvider sp, MockContainer container) ConfigureTestRepositories(
        this IServiceCollection serviceCollection,
        IReadOnlyCollection<LiveEventSeries>? liveEventSeries = null,
        IReadOnlyCollection<User>? users = null,
        IReadOnlyCollection<Role>? roles = null,
        IReadOnlyCollection<TlgAgentRoleBind>? roleBindings = null,
        IReadOnlyCollection<TlgInput>? inputs = null)
    {
        List<LiveEventSeries> defaultSeries = [SeriesX, SeriesY];
        List<User> defaultUsers = [DanielEn, DanielDe];
        List<Role> defaultRoles = [SOpsAdmin_DanielEn_X2024];
        List<TlgAgentRoleBind> defaultRoleBindings = 
            [GetNewRoleBind(SOpsAdmin_DanielEn_X2024, PrivateBotChat_Operations)];
        List<TlgInput> defaultInputs = [];

        var mockContainer = new MockContainer();
        
        serviceCollection = serviceCollection
            .ArrangeTestLiveEventSeriesRepo(liveEventSeries ?? defaultSeries, mockContainer)
            .ArrangeTestUsersRepo(users ?? defaultUsers, mockContainer)
            .ArrangeTestRolesRepo(roles ?? defaultRoles, mockContainer)
            .ArrangeTestRoleBindingsRepo(roleBindings ?? defaultRoleBindings, mockContainer)
            .ArrangeTestTlgInputsRepo(inputs ?? defaultInputs, mockContainer);

        return (serviceCollection.BuildServiceProvider(), mockContainer);
    }

    private static IServiceCollection ArrangeTestLiveEventSeriesRepo(
        this IServiceCollection serviceCollection,
        IReadOnlyCollection<LiveEventSeries> liveEventSeries,
        MockContainer container)
    {
        var mockLiveEventSeriesRepo = new Mock<ILiveEventSeriesRepository>();
        mockLiveEventSeriesRepo
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(liveEventSeries);

        container.Mocks[typeof(ILiveEventSeriesRepository)] = mockLiveEventSeriesRepo;
        var stubLiveEventSeriesRepo = mockLiveEventSeriesRepo.Object;
        
        return serviceCollection.AddScoped<ILiveEventSeriesRepository>(_ => stubLiveEventSeriesRepo);
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
            .ReturnsAsync(roleBindings.Where(tarb => tarb.Status == DbRecordStatus.Active));

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
            .Setup(repo => repo.GetAllAsync(It.IsAny<TlgAgent>()))
            .ReturnsAsync(inputs);

        container.Mocks[typeof(ITlgInputsRepository)] = mockTlgInputsRepo;
        var stubTlgInputsRepo = mockTlgInputsRepo.Object;
        
        return serviceCollection.AddScoped<ITlgInputsRepository>(_ => stubTlgInputsRepo);
    }

    internal record MockContainer
    {
        internal Dictionary<Type, object> Mocks { get; } = new();
    }
}