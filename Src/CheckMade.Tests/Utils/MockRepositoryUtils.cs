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

internal static class MockRepositoryUtils
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
        IReadOnlyCollection<LiveEventVenue>? venues = null,
        IReadOnlyCollection<LiveEvent>? liveEvents = null,
        IReadOnlyCollection<User>? users = null,
        IReadOnlyCollection<Role>? roles = null,
        IReadOnlyCollection<TlgAgentRoleBind>? roleBindings = null,
        IReadOnlyCollection<TlgInput>? inputs = null)
    {
        List<User> defaultUsers = [DanielEn, DanielDe];
        List<Role> defaultRoles = [SOpsAdmin_DanielEn_X2024];
        List<TlgAgentRoleBind> defaultRoleBindings = [];
        List<TlgInput> defaultInputs = [];
        List<LiveEventSeries> defaultSeries = [SeriesX, SeriesY];
        List<LiveEventVenue> defaultVenues = [Venue1, Venue2];
        List<LiveEvent> defaultLiveEvents = [X2024, X2025, Y2024, Y2025];

        var mockContainer = new MockContainer();
        
        serviceCollection = serviceCollection
            .ArrangeMockRolesRepo(roles ?? defaultRoles, mockContainer)
            .ArrangeMockRoleBindingsRepo(roleBindings ?? defaultRoleBindings, mockContainer)
            .ArrangeMockTlgInputsRepo(inputs ?? defaultInputs, mockContainer);

        return (serviceCollection.BuildServiceProvider(), mockContainer);
    }

    private static IServiceCollection ArrangeMockRolesRepo(
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
    
    private static IServiceCollection ArrangeMockRoleBindingsRepo(
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

    
    private static IServiceCollection ArrangeMockTlgInputsRepo(
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