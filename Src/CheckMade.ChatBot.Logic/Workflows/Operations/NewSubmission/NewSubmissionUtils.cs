using System.Collections.Immutable;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem.RoleTypes;
using CheckMade.Common.Domain.Data.Core.GIS;
using CheckMade.Common.Domain.Data.Core.Trades;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Domain.Utils;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.ChatBot.Logic.Workflows.Operations.NewSubmission;

internal static class NewSubmissionUtils
{
    internal static async Task<Option<Geo>> LastKnownLocationAsync(
        Input currentInput, IGeneralWorkflowUtils workflowUtils)
    {
        var lastKnownLocationInput =
            (await workflowUtils.GetRecentLocationHistory(currentInput.Agent))
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
        Input currentInput,
        IAgentRoleBindingsRepository roleBindingsRepo,
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
        Input currentInput,
        IAgentRoleBindingsRepository roleBindingsRepo,
        ILiveEventsRepository liveEventsRepo,
        ITrade trade)
    {
        var liveEventInfo = currentInput.LiveEventContext.GetValueOrThrow();
            
        var currentRole = (await roleBindingsRepo.GetAllActiveAsync())
            .First(arb => arb.Role.Equals(
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
        IReadOnlyCollection<Input> inputs,
        IReadOnlyCollection<ISphereOfAction> spheres) where T : ITrade, new()
    {
        var sphereNames = spheres.Select(static s => s.Name).ToHashSet();
        Func<string, bool> containsSphereName = text => sphereNames.Any(text.Contains);

        var lastSelectedSphereInput =
            inputs.LastOrDefault(i =>
                i.InputType == InputType.TextMessage &&
                containsSphereName(i.Details.Text.GetValueOrThrow()));

        var lastConfirmedSphereInput =
            inputs.LastOrDefault(i =>
                i.InputType == InputType.CallbackQuery &&
                i.Details.ControlPromptEnumCode.IsSome &&
                i.Details.ControlPromptEnumCode.GetValueOrThrow() == (int)ControlPrompts.Yes &&
                containsSphereName(i.Details.Text.GetValueOrThrow()));

        var sphereNameByMessageId = new Dictionary<int, string>();

        if (lastSelectedSphereInput != null)
            sphereNameByMessageId[lastSelectedSphereInput.MessageId] =
                lastSelectedSphereInput.Details.Text.GetValueOrThrow();

        if (lastConfirmedSphereInput != null)
            sphereNameByMessageId[lastConfirmedSphereInput.MessageId] =
                sphereNames.First(sn =>
                    lastConfirmedSphereInput.Details.Text.GetValueOrThrow().Contains(sn));  
        
        return
            spheres.First(s => 
                s.Name == sphereNameByMessageId
                    .MaxBy(static kvp => kvp.Key) // the later of the two, in case the user confirmed AND selected a sphere
                    .Value);
    }

    internal static Type GetLastSubmissionType(IReadOnlyCollection<Input> inputs)
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