using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Utils.GIS;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue;

internal static class NewIssueUtils
{
    internal static async Task<Option<Geo>> LastKnownLocationAsync(
        TlgInput currentInput, IGeneralWorkflowUtils workflowUtils)
    {
        var lastKnownLocationInput =
            (await workflowUtils.GetRecentLocationHistory(currentInput.TlgAgent))
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
            SanitaryTrade => SanitaryTrade.SphereNearnessThresholdInMeters,
            SiteCleanTrade => SiteCleanTrade.SphereNearnessThresholdInMeters,
            _ => throw new InvalidOperationException("Missing switch for ITrade type for nearness threshold")
        };

        var allSpheres = GetAllTradeSpecificSpheres(
            liveEvent ?? throw new InvalidOperationException("LiveEvent missing."),
            trade);
        
        var nearestSphere =
            allSpheres
                .Where(soa =>
                    soa.Details.GeoCoordinates.IsSome &&
                    DistanceFromLastKnownLocation(soa) < tradeSpecificNearnessThreshold)
                .MinBy(DistanceFromLastKnownLocation);

        return
            nearestSphere != null
                ? Option<ISphereOfAction>.Some(nearestSphere)
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
            .ToImmutableArray();

    internal static ISphereOfAction GetLastSelectedSphere<T>(
        IReadOnlyCollection<TlgInput> inputs,
        IReadOnlyCollection<ISphereOfAction> spheres) where T : ITrade, new()
    {
        var sphereNames = spheres.Select(static s => s.Name).ToHashSet();
        Func<string, bool> containsSphereName = text => sphereNames.Any(text.Contains);

        var lastSelectedSphereInput =
            inputs.LastOrDefault(i =>
                i.InputType == TlgInputType.TextMessage &&
                containsSphereName(i.Details.Text.GetValueOrThrow()));

        var lastConfirmedSphereInput =
            inputs.LastOrDefault(i =>
                i.InputType == TlgInputType.CallbackQuery &&
                i.Details.ControlPromptEnumCode.IsSome &&
                i.Details.ControlPromptEnumCode.GetValueOrThrow() == (int)ControlPrompts.Yes &&
                containsSphereName(i.Details.Text.GetValueOrThrow()));

        var sphereNameByMessageId = new Dictionary<int, string>();

        if (lastSelectedSphereInput != null)
            sphereNameByMessageId[lastSelectedSphereInput.TlgMessageId] =
                lastSelectedSphereInput.Details.Text.GetValueOrThrow();

        if (lastConfirmedSphereInput != null)
            sphereNameByMessageId[lastConfirmedSphereInput.TlgMessageId] =
                sphereNames.First(sn =>
                    lastConfirmedSphereInput.Details.Text.GetValueOrThrow().Contains(sn));  
        
        return
            spheres.First(s => 
                s.Name == sphereNameByMessageId
                    .MaxBy(static kvp => kvp.Key) // the later of the two, in case the user confirmed AND selected a sphere
                    .Value);
    }

    internal static Type GetLastIssueType(IReadOnlyCollection<TlgInput> inputs)
    {
        return 
            inputs
                .Last(static i =>
                    i.Details.DomainTerm.IsSome &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IIssue)))
                .Details.DomainTerm.GetValueOrThrow()
                .TypeValue!;
    }
}