using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;
using CheckMade.Common.Utils.GIS;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow;

internal class NewIssueWorkflow(
    ILiveEventsRepository liveEventsRepo,
    ILogicUtils logicUtils,
    IDomainGlossary glossary)
    : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        var currentRole = currentInput.OriginatorRole.GetValueOrThrow();
        
        if (currentInput.ResultantWorkflow.IsNone)
        {
            return await NewIssueWorkflowInitAsync(currentInput, currentRole);
        }

        var interactiveHistory =
            await logicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);

        var lastInput =
            interactiveHistory
                .SkipLast(1) // skip currentInput
                .Last(); // no OrDefault: at this point we are certain the history has at least 2 inputs!

        var currentState =
            glossary.GetDtType(
                lastInput
                    .ResultantWorkflow.GetValueOrThrow()
                    .InStateId);

        switch (currentState.Name)
        {
            case nameof(NewIssueInitialTradeUnknown):
                
                return await new NewIssueInitialTradeUnknown(glossary)
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync();
            
            case nameof(NewIssueInitialSphereKnown):
                
                var trade = IsCurrentRoleTradeSpecific(currentRole)
                    ? currentRole.RoleType.GetTradeInstance().GetValueOrThrow()
                    : GetLastUserProvidedTrade();

                var sphere = 
                    (await SphereNearCurrentUserAsync(currentInput, trade))
                    .GetValueOrThrow(); 
                
                return await new NewIssueInitialSphereKnown(trade, sphere)
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync();
            
            case nameof(NewIssueInitialSphereUnknown):
                
                return await new NewIssueInitialSphereUnknown()
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync();
            
            case nameof(NewIssueSphereConfirmed):
                
                return await new NewIssueSphereConfirmed()
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync();
            
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
            var initialTradeUnknown = new NewIssueInitialTradeUnknown(glossary);

            return new WorkflowResponse(
                initialTradeUnknown.MyPrompt(),
                glossary.GetId(initialTradeUnknown.GetType()));
        }

        var trade = currentRole.RoleType.GetTradeInstance().GetValueOrThrow();

        return trade.DividesLiveEventIntoSpheresOfAction switch
        {
            true => (await SphereNearCurrentUserAsync(currentInput, trade)).Match(
                sphere => new WorkflowResponse(
                    new NewIssueInitialSphereKnown(trade, sphere).MyPrompt(),
                    glossary.GetId(typeof(NewIssueInitialSphereKnown))),

                () => new WorkflowResponse(
                    new NewIssueInitialSphereUnknown().MyPrompt(),
                    glossary.GetId(typeof(NewIssueInitialSphereUnknown)))),

            _ => new WorkflowResponse(
                new NewIssueSphereConfirmed().MyPrompt(),
                glossary.GetId(typeof(NewIssueSphereConfirmed)))
        };
    }

    private static bool IsCurrentRoleTradeSpecific(IRoleInfo currentRole) =>
        currentRole
            .RoleType
            .GetTradeInstance().IsSome;
    
    private async Task<Option<ISphereOfAction>> SphereNearCurrentUserAsync(
        TlgInput currentInput,
        ITrade trade)
    {
        var lastKnownLocationInput =
            (await logicUtils.GetRecentLocationHistory(currentInput.TlgAgent))
            .LastOrDefault();

        if (lastKnownLocationInput is null)
            return Option<ISphereOfAction>.None();

        var lastKnownLocation =
            lastKnownLocationInput.Details.GeoCoordinates.GetValueOrThrow();

        var liveEvent =
            await liveEventsRepo.GetAsync(
                currentInput.LiveEventContext.GetValueOrThrow());

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