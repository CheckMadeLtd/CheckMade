using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Data.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem.RoleTypes;
using CheckMade.Common.DomainModel.Data.Core.GIS;
using CheckMade.Common.DomainModel.Data.Core.Trades;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using CheckMade.Common.DomainModel.Interfaces.Persistence.Core;
using CheckMade.Common.DomainModel.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission;

internal static class NewSubmissionUtils
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

    internal static async Task<Option<ISphereOfAction>> SphereNearCurrentUserAsync(
        ILiveEventInfo liveEventInfo,
        ILiveEventsRepository liveEventsRepo,
        Geo lastKnownLocation,
        ITrade trade,
        TlgInput currentInput,
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        bool filterAssignedSpheresIfAny = true)
    {
        var tradeSpecificNearnessThreshold = trade switch
        {
            SanitaryTrade => SanitaryTrade.SphereNearnessThresholdInMeters,
            SiteCleanTrade => SiteCleanTrade.SphereNearnessThresholdInMeters,
            _ => throw new InvalidOperationException("Missing switch for ITrade type for nearness threshold")
        };

        var allRelevantSpheres = filterAssignedSpheresIfAny
            ? await AssignedSpheresOrAllAsync(
                currentInput, roleBindingsRepo, liveEventsRepo, trade)
            : await GetAllTradeSpecificSpheresAsync(
                trade, liveEventInfo, liveEventsRepo);
        
        var nearestSphere =
            allRelevantSpheres
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

    internal static async Task<IReadOnlyCollection<ISphereOfAction>> AssignedSpheresOrAllAsync(
        TlgInput currentInput,
        ITlgAgentRoleBindingsRepository roleBindingsRepo,
        ILiveEventsRepository liveEventsRepo,
        ITrade trade)
    {
        var liveEventInfo = currentInput.LiveEventContext.GetValueOrThrow();
            
        var currentRole = (await roleBindingsRepo.GetAllActiveAsync())
            .First(tarb => tarb.Role.Equals(
                currentInput.OriginatorRole.GetValueOrThrow()))
            .Role;
        var currentRoleType = currentRole.RoleType;
        
        // This is taking names from any assigned spheres, also those for other trades
        // This might lead to a bug in case different trades end up with spheres of the same name
        var assignedSpheres =
            currentRoleType is TradeTeamLead<SanitaryTrade> or TradeTeamLead<SiteCleanTrade>
                ? currentRole.AssignedToSpheres
                : Array.Empty<ISphereOfAction>();

        var allCurrentTradeSpheres = 
            (await GetAllTradeSpecificSpheresAsync(trade, liveEventInfo, liveEventsRepo))
            .ToArray();
        
        return
            assignedSpheres.Count != 0
                ? allCurrentTradeSpheres.Intersect(assignedSpheres).ToArray()
                : allCurrentTradeSpheres;
    }
    
    internal static async Task<IReadOnlyCollection<ISphereOfAction>>
        GetAllTradeSpecificSpheresAsync(
            ITrade trade,
            ILiveEventInfo liveEventInfo,
            ILiveEventsRepository liveEventsRepo)
    {
        return (await liveEventsRepo.GetAsync(liveEventInfo))!
            .DivIntoSpheres
            .Where(soa => soa.GetTradeType() == trade.GetType())
            .ToImmutableArray();
    }

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

    internal static Type GetLastSubmissionType(IReadOnlyCollection<TlgInput> inputs)
    {
        return 
            inputs
                .Last(static i =>
                    i.Details.DomainTerm.IsSome &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(ISubmission)))
                .Details.DomainTerm.GetValueOrThrow()
                .TypeValue!;
    }
}