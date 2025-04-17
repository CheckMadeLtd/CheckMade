using System.Collections.Immutable;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Model.Core.LiveEvents.Concrete.SphereOfActionDetails.Facilities;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration.Persistence;

public sealed class LiveEventsRepositoryTests
{
    private IServiceProvider? _services;

    [Fact]
    public async Task GetAsync_ReturnsCorrectLiveEventObjectGraph_ForX2024TestEvent()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
    
        var liveEventsRepo = _services.GetRequiredService<ILiveEventsRepository>();
    
        var liveEventGraph = await liveEventsRepo.GetAsync(X2024);
        var allSphereNames = liveEventGraph!.DivIntoSpheres.Select(static s => s.Name).ToList(); 

        Assert.NotNull(liveEventGraph);
        
        Assert.Equal(X2024.Name, liveEventGraph.Name);
        Assert.Equivalent(Venue1, liveEventGraph.AtVenue);

        var actualLiveEventRoleTokens = 
            liveEventGraph.WithRoles
                .Select(static r => r.Token)
                .ToImmutableList(); 
        
        Assert.Equal(X2024.WithRoles.Count, actualLiveEventRoleTokens.Count);
        Assert.Contains(X2024.WithRoles.First().Token, actualLiveEventRoleTokens);
        Assert.Contains(X2024.WithRoles.Last().Token,actualLiveEventRoleTokens);

        Assert.Contains(Sphere1_AtX2024.Name, allSphereNames);
        Assert.Contains(Sphere2_AtX2024.Name, allSphereNames);
        Assert.Contains(Sphere4_AtX2024.Name, allSphereNames);
        
        Assert.Equal(4, allSphereNames.Count);
        
        Assert.Equal(
            Sphere1_AtX2024.GetTradeType(),
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere1_AtX2024.Name)
                .GetTradeType());
        Assert.Equal(
            Sphere2_AtX2024.GetTradeType(),
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere2_AtX2024.Name)
                .GetTradeType());
        Assert.Equal(
            Sphere4_AtX2024.GetTradeType(),
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere4_AtX2024.Name)
                .GetTradeType());

        Assert.Equivalent(
            Location_Dassel,
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere1_AtX2024.Name)
                .Details.GeoCoordinates.GetValueOrThrow());
        Assert.Equivalent(
            Location_4cc,
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere2_AtX2024.Name)
                .Details.GeoCoordinates.GetValueOrThrow());
        Assert.Equivalent(
            Option<Geo>.None(),
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere4_AtX2024.Name)
                .Details.GeoCoordinates);

        List<DomainTerm> expectedFacilitiesAtX2024Sphere1 =
        [
            Dt(typeof(GeneralMisc)),
            Dt(typeof(Shower)),
            Dt(typeof(Toilet))
        ];

        var actualFacilitiesAtX2024Sphere1 =
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere1_AtX2024.Name)
                .Details.AvailableFacilities;

        foreach (var item in expectedFacilitiesAtX2024Sphere1)
        {
            Assert.Contains(item, actualFacilitiesAtX2024Sphere1);
        }

        var actualFacilitiesAtX2024Sphere2 =
            liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere2_AtX2024.Name)
                .Details.AvailableFacilities; 
        
        Assert.Contains(Dt(typeof(GeneralMisc)), actualFacilitiesAtX2024Sphere2);
        Assert.DoesNotContain(Dt(typeof(Shower)), actualFacilitiesAtX2024Sphere2);
        Assert.DoesNotContain(Dt(typeof(Toilet)), actualFacilitiesAtX2024Sphere2);

        List<DomainTerm> expectedConsumableItemsAtX2024Sphere1 = 
        [
            Dt(ConsumablesItem.ToiletPaper),
            Dt(ConsumablesItem.PaperTowels),
            Dt(ConsumablesItem.Soap)
        ];

        var actualConsumableItemsAtX2024Sphere1 = 
            ((SanitaryCampDetails)liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere1_AtX2024.Name)
                .Details)
            .AvailableConsumables;

        foreach (var item in expectedConsumableItemsAtX2024Sphere1)
        {
            Assert.Contains(item, actualConsumableItemsAtX2024Sphere1);
        }
        
        var actualConsumableItemsAtX2024Sphere2 = 
            ((SanitaryCampDetails)liveEventGraph.DivIntoSpheres
                .First(static s => s.Name == Sphere2_AtX2024.Name)
                .Details)
            .AvailableConsumables;
        
        Assert.Contains(Dt(ConsumablesItem.ToiletPaper), actualConsumableItemsAtX2024Sphere2);
        Assert.DoesNotContain(Dt(ConsumablesItem.PaperTowels), actualConsumableItemsAtX2024Sphere2);
        Assert.DoesNotContain(Dt(ConsumablesItem.Soap), actualConsumableItemsAtX2024Sphere2);
    }
}