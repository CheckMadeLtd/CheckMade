using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Utils.GIS;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue;

internal static class NewIssueUtils
{
    internal static async Task<Option<Geo>> LastKnownLocationAsync(
        TlgInput currentInput, IGeneralWorkflowUtils generalWorkflowUtils)
    {
        var lastKnownLocationInput =
            (await generalWorkflowUtils.GetRecentLocationHistory(currentInput.TlgAgent))
            .LastOrDefault();

        return lastKnownLocationInput is null 
            ? Option<Geo>.None() 
            : lastKnownLocationInput.Details.GeoCoordinates.GetValueOrThrow();
    }

    internal static Option<ISphereOfAction> SphereNearCurrentUser(
        LiveEvent liveEvent,
        Geo lastKnownLocation,
        ITrade trade)
    {
        var tradeSpecificNearnessThreshold = trade switch
        {
            SaniCleanTrade => SaniCleanTrade.SphereNearnessThresholdInMeters,
            SiteCleanTrade => SiteCleanTrade.SphereNearnessThresholdInMeters,
            _ => throw new InvalidOperationException("Missing switch for ITrade type for nearness threshold")
        };

        var allSpheres = GetAllTradeSpecificSpheres(
            liveEvent ?? throw new InvalidOperationException("LiveEvent missing."),
            trade);

        var nearSphere =
            allSpheres
                .Where(soa =>
                    soa.Details.GeoCoordinates.IsSome &&
                    DistanceFromLastKnownLocation(soa) < tradeSpecificNearnessThreshold)
                .MinBy(DistanceFromLastKnownLocation);

        return
            nearSphere != null
                ? Option<ISphereOfAction>.Some(nearSphere)
                : Option<ISphereOfAction>.None();

        double DistanceFromLastKnownLocation(ISphereOfAction soa) =>
            soa.Details.GeoCoordinates.GetValueOrThrow()
                .MetersAwayFrom(lastKnownLocation);
    }

    internal static IReadOnlyCollection<ISphereOfAction>
        GetAllTradeSpecificSpheres(LiveEvent liveEvent, ITrade trade) =>
        liveEvent
            .DivIntoSpheres
            .Where(soa => soa.GetTradeType() == trade.GetType())
            .ToImmutableReadOnlyCollection();

    internal static ISphereOfAction GetLastSelectedSphere(
        IReadOnlyCollection<TlgInput> inputs,
        IReadOnlyCollection<ISphereOfAction> spheres)
    {
        var lastSelectedSphereName =
            inputs.Last(i =>
                    i.Details.Text.IsSome &&
                    spheres.Select(s => s.Name)
                        .Contains(i.Details.Text.GetValueOrThrow()))
                .Details.Text.GetValueOrThrow();

        return
            spheres
                .First(s => 
                    s.Name.Equals(lastSelectedSphereName));
    }

    internal static Type GetLastIssueType(IReadOnlyCollection<TlgInput> inputs)
    {
        return 
            inputs
                .Last(i =>
                    i.Details.DomainTerm.IsSome &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IIssue)))
                .Details.DomainTerm.GetValueOrThrow()
                .TypeValue!;
    }
}