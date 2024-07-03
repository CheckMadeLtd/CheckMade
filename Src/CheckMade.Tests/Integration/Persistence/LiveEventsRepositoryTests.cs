using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public class LiveEventsRepositoryTests
{
    private IServiceProvider? _services;

    [Fact]
    public async Task GetAsync_ReturnsCorrectLiveEventObjectGraph_ForX2024TestEvent()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
    
        var liveEventsRepo = _services.GetRequiredService<ILiveEventsRepository>();
    
        var liveEventGraph = await liveEventsRepo.GetAsync(X2024);
        
        Assert.Equal(
            X2024.Name,
            liveEventGraph.Name);
        
        Assert.Equal(
            Sphere1_AtX2024.Name,
            liveEventGraph.DivIntoSpheres.ToList()[0].Name);
        Assert.Equivalent(
            Sphere1_AtX2024.Trade,
            liveEventGraph.DivIntoSpheres.ToList()[0].Trade);
        Assert.Equivalent(
            Sphere1_Location,
            liveEventGraph.DivIntoSpheres.ToList()[0].Details.Location.GetValueOrThrow());
        
        
        Assert.Equal(
            Sphere2_AtX2024.Name,
            liveEventGraph.DivIntoSpheres.ToList()[1].Name);
        Assert.Equal(
            Sphere3_AtX2024.Name,
            liveEventGraph.DivIntoSpheres.ToList()[2].Name);
        Assert.Equivalent(
            Sphere3_AtX2024.Trade,
            liveEventGraph.DivIntoSpheres.ToList()[2].Trade);
        
        Assert.Equivalent(
            Venue1,
            liveEventGraph.AtVenue);
        
        Assert.Equal(
            X2024.WithRoles.Count,
            liveEventGraph.WithRoles.Count);
    }
}