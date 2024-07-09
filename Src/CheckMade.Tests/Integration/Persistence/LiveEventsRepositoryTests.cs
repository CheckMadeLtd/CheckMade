using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
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
        var allSphereNames = liveEventGraph!.DivIntoSpheres.Select(s => s.Name).ToList(); 

        Assert.NotNull(liveEventGraph);
        
        Assert.Equal(X2024.Name, liveEventGraph.Name);
        Assert.Equivalent(Venue1, liveEventGraph.AtVenue);
        
        Assert.Equal(X2024.WithRoles.Count, liveEventGraph.WithRoles.Count);
        Assert.Contains(
            X2024.WithRoles.First().Token,
            liveEventGraph.WithRoles.Select(r => r.Token));
        Assert.Contains(
            X2024.WithRoles.Last().Token,
            liveEventGraph.WithRoles.Select(r => r.Token));

        Assert.Contains(Sphere1_AtX2024.Name, allSphereNames);
        Assert.Contains(Sphere2_AtX2024.Name, allSphereNames);
        Assert.Contains(Sphere3_AtX2024.Name, allSphereNames);
        
        Assert.Equivalent(
            Sphere1_AtX2024.Trade,
            liveEventGraph.DivIntoSpheres
                .First(s => s.Name == Sphere1_AtX2024.Name)
                .Trade);
        
        Assert.Equivalent(
            Sphere2_AtX2024.Trade,
            liveEventGraph.DivIntoSpheres
                .First(s => s.Name == Sphere2_AtX2024.Name)
                .Trade);
        
        Assert.Equivalent(
            Sphere3_AtX2024.Trade,
            liveEventGraph.DivIntoSpheres
                .First(s => s.Name == Sphere3_AtX2024.Name)
                .Trade);

        Assert.Equivalent(
            Sphere1_Location,
            liveEventGraph.DivIntoSpheres
                .First(s => s.Name == Sphere1_AtX2024.Name)
                .Details.GeoCoordinates.GetValueOrThrow());
        
        Assert.Equivalent(
            Sphere2_Location,
            liveEventGraph.DivIntoSpheres
                .First(s => s.Name == Sphere2_AtX2024.Name)
                .Details.GeoCoordinates.GetValueOrThrow());

        Assert.Equivalent(
            Option<Geo>.None(),
            liveEventGraph.DivIntoSpheres
                .First(s => s.Name == Sphere3_AtX2024.Name)
                .Details.GeoCoordinates);
    }
}