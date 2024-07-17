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
        throw new NotImplementedException();
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
        {
            return await NewIssueWorkflowInitAsync(currentInput, currentRole);
        }

        var currentState =
            Glossary.GetDtType(
                lastInput
                    .ResultantWorkflow.GetValueOrThrow()
                    .InStateId);

        var liveEvent = await LiveEventsRepo.GetAsync(
            currentInput.LiveEventContext.GetValueOrThrow());
        
        var trade = IsCurrentRoleTradeSpecific(currentRole)
            ? currentRole.RoleType.GetTradeInstance().GetValueOrThrow()
            : GetLastUserProvidedTrade();

        switch (currentState.Name)
        {
            case nameof(NewIssueInitialTradeUnknown):
                
                return await new NewIssueInitialTradeUnknown(Glossary)
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(currentInput);
            
            case nameof(NewIssueInitialSphereKnown):
                
                var lastKnownLocation = await LastKnownLocationAsync(currentInput);
                
                var sphere = lastKnownLocation.IsSome
                    ? SphereNearCurrentUser(liveEvent!, lastKnownLocation.GetValueOrThrow(), trade)
                    : Option<ISphereOfAction>.None();

                if (sphere.IsNone)
                {
                    // ToDo: break? Handle case where user has moved away since confirming Sphere in last step! 
                    // It's an edge case. I think should lead back to SphereUnknown state.
                    // Or maybe pass Option<ISphereOfAction> to the constructor and handle it there? 
                }
                
                return await new NewIssueInitialSphereKnown(trade, sphere.GetValueOrThrow())
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(currentInput);
            
            case nameof(NewIssueInitialSphereUnknown):
                
                return await new NewIssueInitialSphereUnknown(trade, liveEvent!, Glossary)
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(currentInput);
            
            case nameof(NewIssueSphereConfirmed<ITrade>):

                return trade switch
                {
                    SaniCleanTrade => await new NewIssueSphereConfirmed<SaniCleanTrade>(Glossary)
                        .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(currentInput),
                    SiteCleanTrade => await new NewIssueSphereConfirmed<SiteCleanTrade>(Glossary)
                        .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(currentInput),
                    _ => throw new InvalidOperationException($"Unhandled type of {nameof(trade)}: '{trade.GetType()}'")
                };
            
            default:
                
                throw new InvalidOperationException(
                    $"Lack of handling of state '{currentState.Name}' in '{nameof(NewIssueWorkflow)}'");
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
            var initialTradeUnknown = new NewIssueInitialTradeUnknown(Glossary);

            return new WorkflowResponse(
                initialTradeUnknown.MyPrompt(),
                Glossary.GetId(initialTradeUnknown.GetType()));
        }

        var trade = currentRole.RoleType.GetTradeInstance().GetValueOrThrow();
        
        if (trade.DividesLiveEventIntoSpheresOfAction)
        {
            var liveEvent = await LiveEventsRepo.GetAsync(
                currentInput.LiveEventContext.GetValueOrThrow());

            var lastKnownLocation = await LastKnownLocationAsync(currentInput);

            var sphere = lastKnownLocation.IsSome
                ? SphereNearCurrentUser(liveEvent!, lastKnownLocation.GetValueOrThrow(), trade)
                : Option<ISphereOfAction>.None();

            return sphere.Match(
                soa => new WorkflowResponse(
                    new NewIssueInitialSphereKnown(trade, soa).MyPrompt(),
                    Glossary.GetId(typeof(NewIssueInitialSphereKnown))),
                () => new WorkflowResponse(
                    new NewIssueInitialSphereUnknown(trade, liveEvent!, Glossary).MyPrompt(),
                    Glossary.GetId(typeof(NewIssueInitialSphereUnknown))));
        }

        return trade switch
        {
            SaniCleanTrade => new WorkflowResponse(
                new NewIssueSphereConfirmed<SaniCleanTrade>(Glossary).MyPrompt(),
                Glossary.GetId(typeof(NewIssueSphereConfirmed<SaniCleanTrade>))),
            
            SiteCleanTrade => new WorkflowResponse(
                new NewIssueSphereConfirmed<SiteCleanTrade>(Glossary).MyPrompt(),
                Glossary.GetId(typeof(NewIssueSphereConfirmed<SiteCleanTrade>))),
            
            _ => throw new InvalidOperationException($"Unhandled type of {nameof(trade)}: '{trade.GetType()}'")
        };
    }

    private static bool IsCurrentRoleTradeSpecific(IRoleInfo currentRole) =>
        currentRole
            .RoleType
            .GetTradeInstance().IsSome;

    private async Task<Option<Geo>> LastKnownLocationAsync(TlgInput currentInput)
    {
        var lastKnownLocationInput =
            (await LogicUtils.GetRecentLocationHistory(currentInput.TlgAgent))
            .LastOrDefault();

        return lastKnownLocationInput is null 
            ? Option<Geo>.None() 
            : lastKnownLocationInput.Details.GeoCoordinates.GetValueOrThrow();
    }
    
    private static Option<ISphereOfAction> SphereNearCurrentUser(
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
                .Where(soa => DistanceFromLastKnownLocation(soa) < tradeSpecificNearnessThreshold)
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