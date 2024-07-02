using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public class LiveEventSeriesRepositoryTests
{
    private IServiceProvider? _services;

    [Fact]
    public async Task GetAllAsync_ReturnsLiveEventSeriesObjectGraph_WithNestedOneToManyAggregates()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();

        var liveEventSeriesRepo = _services.GetRequiredService<ILiveEventSeriesRepository>();

        var liveEventSeriesGraph =
            (await liveEventSeriesRepo.GetAllAsync())
            .ToList();
        
        Assert.Equal(
            SeriesX.Name,
            liveEventSeriesGraph[0].Name);
        Assert.Equal(
            X2024.Name,
            liveEventSeriesGraph[0].LiveEvents.First().Name);
        Assert.Equal(
            X2025.Name,
            liveEventSeriesGraph[0].LiveEvents.Last().Name);
        Assert.Equal(
            Sphere1_AtX2024.Name,
            liveEventSeriesGraph[0].LiveEvents.First().DivIntoSpheres.ToList()[0].Name);
        Assert.Equivalent(
            Sphere1_AtX2024.Trade,
            liveEventSeriesGraph[0].LiveEvents.First().DivIntoSpheres.ToList()[0].Trade);
        Assert.Equal(
            Sphere2_AtX2024.Name,
            liveEventSeriesGraph[0].LiveEvents.First().DivIntoSpheres.ToList()[1].Name);
        Assert.Equal(
            Sphere3_AtX2024.Name,
            liveEventSeriesGraph[0].LiveEvents.First().DivIntoSpheres.ToList()[2].Name);
        Assert.Equivalent(
            Sphere3_AtX2024.Trade,
            liveEventSeriesGraph[0].LiveEvents.First().DivIntoSpheres.ToList()[2].Trade);

        
        Assert.Equal(
            SeriesY,
            liveEventSeriesGraph[1]);
        Assert.Equal(
            Y2024,
            liveEventSeriesGraph[1].LiveEvents.First());
    }
}