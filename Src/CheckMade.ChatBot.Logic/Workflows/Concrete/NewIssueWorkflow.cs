using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;
using CheckMade.Common.Utils.GIS;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow;

internal record NewIssueWorkflow(
        ILiveEventsRepository LiveEventsRepo,
        ILogicUtils LogicUtils,
        IDomainGlossary Glossary)
    : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        // ToDo: implement correctly once we have entire workflow.
        return false;
    }

    public async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        var currentRole = currentInput.OriginatorRole.GetValueOrThrow();
        
        var interactiveHistory =
            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);

        var lastInput =
            interactiveHistory
                .SkipLast(1) // skip currentInput
                .LastOrDefault();

        if (lastInput is null)
            return await NewIssueWorkflowInitAsync(currentInput, currentRole);

        var currentStateName = 
            await LogicUtils.GetLastStateName(currentInput);

        ITrade trade;
        
        switch (currentStateName)
        {
            case nameof(NewIssueTradeSelection):
                
                return await new NewIssueTradeSelection(
                        Glossary, LiveEventsRepo, LogicUtils)
                    .GetWorkflowResponseAsync(currentInput);
            
            case nameof(NewIssueSphereConfirmation<ITrade>):
                
                var liveEvent = (await LiveEventsRepo.GetAsync(
                    currentInput.LiveEventContext.GetValueOrThrow()))!;
        
                var lastKnownLocation = await LastKnownLocationAsync(currentInput, LogicUtils);
                
                var sphere = lastKnownLocation.IsSome
                    ? SphereNearCurrentUser(liveEvent, lastKnownLocation.GetValueOrThrow(), GetCurrentTrade())
                    : Option<ISphereOfAction>.None();

                if (sphere.IsNone)
                {
                    // ToDo: break? Handle case where user has moved away since confirming Sphere in last step! 
                    // It's an edge case. I think should lead back to SphereUnknown state.
                    // Or maybe pass Option<ISphereOfAction> to the constructor and handle it there? 
                }

                trade = GetCurrentTrade();
                
                return trade switch
                {
                    SaniCleanTrade => 
                        await new NewIssueSphereConfirmation<SaniCleanTrade>(
                            sphere.GetValueOrThrow(), LiveEventsRepo, Glossary, LogicUtils)
                        .GetWorkflowResponseAsync(currentInput),
                    
                    SiteCleanTrade => 
                        await new NewIssueSphereConfirmation<SiteCleanTrade>(
                            sphere.GetValueOrThrow(), LiveEventsRepo, Glossary, LogicUtils)
                        .GetWorkflowResponseAsync(currentInput),
                    
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade.GetType()}'")
                };
            
            case nameof(NewIssueSphereSelection<ITrade>):
                
                var liveEventInfo = currentInput.LiveEventContext.GetValueOrThrow();
                trade = GetCurrentTrade();

                return trade switch
                {
                    SaniCleanTrade => 
                        await new NewIssueSphereSelection<SaniCleanTrade>(
                            liveEventInfo, LiveEventsRepo, Glossary, LogicUtils)
                        .GetWorkflowResponseAsync(currentInput),
                    
                    SiteCleanTrade => 
                        await new NewIssueSphereSelection<SiteCleanTrade>(
                            liveEventInfo, LiveEventsRepo, Glossary, LogicUtils)
                        .GetWorkflowResponseAsync(currentInput),
                    
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade.GetType()}'")
                };
            
            case nameof(NewIssueTypeSelection<ITrade>):

                trade = GetCurrentTrade();
                
                return trade switch
                {
                    SaniCleanTrade => 
                        await new NewIssueTypeSelection<SaniCleanTrade>(Glossary, LogicUtils)
                            .GetWorkflowResponseAsync(currentInput),
                    
                    SiteCleanTrade => 
                        await new NewIssueTypeSelection<SiteCleanTrade>(Glossary, LogicUtils)
                            .GetWorkflowResponseAsync(currentInput),
                    
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade.GetType()}'")
                };
            
            case nameof(NewIssueConsumablesSelection<ITrade>):

                trade = GetCurrentTrade();
                
                return trade switch
                {
                    SaniCleanTrade => 
                        await new NewIssueConsumablesSelection<SaniCleanTrade>(
                                Glossary, interactiveHistory, LogicUtils)
                        .GetWorkflowResponseAsync(currentInput),
                    
                    SiteCleanTrade => 
                        await new NewIssueConsumablesSelection<SiteCleanTrade>(
                                Glossary, interactiveHistory, LogicUtils)
                            .GetWorkflowResponseAsync(currentInput),
                    
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade.GetType()}'")
                };
            
            case nameof(NewIssueFacilitySelection<ITrade>):

                trade = GetCurrentTrade();
                
                return trade switch
                {
                    SaniCleanTrade =>
                        await new NewIssueFacilitySelection<SaniCleanTrade>(Glossary, LogicUtils)
                            .GetWorkflowResponseAsync(currentInput),
                    
                    SiteCleanTrade =>
                        await new NewIssueFacilitySelection<SiteCleanTrade>(Glossary, LogicUtils)
                            .GetWorkflowResponseAsync(currentInput),
                    
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade.GetType()}'")
                };

            case nameof(NewIssueEvidenceEntry<ITrade>):

                trade = GetCurrentTrade();
                
                return trade switch
                {
                    SaniCleanTrade => 
                        await new NewIssueEvidenceEntry<SaniCleanTrade>(Glossary, LogicUtils)
                            .GetWorkflowResponseAsync(currentInput),

                    SiteCleanTrade => 
                        await new NewIssueEvidenceEntry<SiteCleanTrade>(Glossary, LogicUtils)
                            .GetWorkflowResponseAsync(currentInput),
                    
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade.GetType()}'")
                };
            
            default:
                
                throw new InvalidOperationException(
                    $"Lack of handling of state '{currentStateName}' in '{nameof(NewIssueWorkflow)}'");
        }

        ITrade GetCurrentTrade()
        {
            return IsCurrentRoleTradeSpecific(currentRole)
                ? currentRole.RoleType.GetTradeInstance().GetValueOrThrow()
                : GetLastUserProvidedTrade();
        }
        
        ITrade GetLastUserProvidedTrade()
        {
            var tradeType = interactiveHistory
                .Last(i =>
                    i.Details.DomainTerm.IsSome &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(ITrade)))
                .Details.DomainTerm.GetValueOrThrow().TypeValue!;

            return (ITrade)Activator.CreateInstance(tradeType)!;
        }
    }

    private async Task<Result<WorkflowResponse>> NewIssueWorkflowInitAsync(
        TlgInput currentInput, 
        IRoleInfo currentRole)
    {
        if (!IsCurrentRoleTradeSpecific(currentRole))
        {
            return await WorkflowResponse.CreateAsync(
                new NewIssueTradeSelection(Glossary, LiveEventsRepo, LogicUtils));
        }

        var trade = currentRole.RoleType.GetTradeInstance().GetValueOrThrow();
        
        if (trade.DividesLiveEventIntoSpheresOfAction)
        {
            var liveEvent = (await LiveEventsRepo.GetAsync(
                currentInput.LiveEventContext.GetValueOrThrow()))!;
            
            var lastKnownLocation = await LastKnownLocationAsync(currentInput, LogicUtils);

            var sphere = lastKnownLocation.IsSome
                ? SphereNearCurrentUser(liveEvent, lastKnownLocation.GetValueOrThrow(), trade)
                : Option<ISphereOfAction>.None();

            return await sphere.Match(
                soa => trade switch
                {
                    SaniCleanTrade => WorkflowResponse.CreateAsync(
                        new NewIssueSphereConfirmation<SaniCleanTrade>(
                            soa, LiveEventsRepo, Glossary, LogicUtils)),
                    SiteCleanTrade => WorkflowResponse.CreateAsync(
                        new NewIssueSphereConfirmation<SiteCleanTrade>(
                            soa, LiveEventsRepo, Glossary, LogicUtils)),
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
                },
                () => trade switch
                {
                    SaniCleanTrade => WorkflowResponse.CreateAsync(
                        new NewIssueSphereSelection<SaniCleanTrade>(
                            liveEvent, LiveEventsRepo, Glossary, LogicUtils)),
                    SiteCleanTrade => WorkflowResponse.CreateAsync(
                        new NewIssueSphereSelection<SiteCleanTrade>(
                            liveEvent, LiveEventsRepo, Glossary, LogicUtils)),
                    _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
                });
        }

        return trade switch
        {
            SaniCleanTrade => 
                await WorkflowResponse.CreateAsync(
                    new NewIssueTypeSelection<SaniCleanTrade>(Glossary, LogicUtils)),
            
            SiteCleanTrade => 
                await WorkflowResponse.CreateAsync(
                    new NewIssueTypeSelection<SiteCleanTrade>(Glossary, LogicUtils)),
            
            _ => throw new InvalidOperationException(
                $"Unhandled type of {nameof(trade)}: '{trade.GetType()}'")
        };
    }

    private static bool IsCurrentRoleTradeSpecific(IRoleInfo currentRole) =>
        currentRole
            .RoleType
            .GetTradeInstance().IsSome;

    internal static async Task<Option<Geo>> LastKnownLocationAsync(
        TlgInput currentInput, ILogicUtils logicUtils)
    {
        var lastKnownLocationInput =
            (await logicUtils.GetRecentLocationHistory(currentInput.TlgAgent))
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

        var allSpheres =
            GetAllTradeSpecificSpheres(
                liveEvent ?? throw new InvalidOperationException("LiveEvent missing."),
                trade);

        var nearSphere =
            allSpheres
                .Where(soa =>
                    DistanceFromLastKnownLocation(soa) < tradeSpecificNearnessThreshold)
                .MinBy(DistanceFromLastKnownLocation);

        return
            nearSphere != null
                ? Option<ISphereOfAction>.Some(nearSphere)
                : Option<ISphereOfAction>.None();

        double DistanceFromLastKnownLocation(ISphereOfAction soa) =>
            soa.Details.GeoCoordinates.GetValueOrThrow()
                .MetersAwayFrom(lastKnownLocation);
        
        static IReadOnlyCollection<ISphereOfAction>
            GetAllTradeSpecificSpheres(LiveEvent liveEvent, ITrade trade) =>
            liveEvent
                .DivIntoSpheres
                .Where(soa => soa.GetTradeType() == trade.GetType())
                .ToImmutableReadOnlyCollection();
    }
}