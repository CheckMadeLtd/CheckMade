using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;
using CheckMade.Common.Utils.GIS;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow;

internal class NewIssueWorkflow(
        INewIssueInitialTradeUnknown initialTradeUnknown,
        INewIssueInitialSphereKnown initialSphereKnown,
        INewIssueInitialSphereUnknown initialSphereUnknown,
        INewIssueSphereConfirmed sphereConfirmed,
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
        if (currentInput.ResultantWorkflow.IsNone)
        {
            return await NewIssueWorkflowInitAsync(currentInput);
        }

        var lastInput =
            (await logicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput))
            .SkipLast(1) // skip currentInput
            .Last(); // no OrDefault: at this point we are certain the history has at least 2 inputs!

        var currentState = 
            glossary.GetDtType(
                lastInput
                    .ResultantWorkflow.GetValueOrThrow()
                    .InStateId);
        
        return currentState.Name switch
        {
            nameof(NewIssueInitialTradeUnknown) => 
                await initialTradeUnknown
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(),
            
            nameof(NewIssueInitialSphereKnown) =>
                await initialSphereKnown
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(),
            
            nameof(NewIssueInitialSphereUnknown) =>
                await initialSphereUnknown
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(),
            
            nameof(NewIssueSphereConfirmed) =>
                await sphereConfirmed
                    .ProcessAnswerToMyPromptToGetNextStateWithItsPromptAsync(),
            
            _ => throw new InvalidOperationException(
                $"Lack of handling of state '{currentState.Name}' in '{nameof(NewIssueWorkflow)}'")
        };
    }

    private async Task<Result<WorkflowResponse>> NewIssueWorkflowInitAsync(TlgInput currentInput)
    {
            var currentRole = currentInput.OriginatorRole.GetValueOrThrow(); 
            
            var isCurrentRoleTradeSpecific = 
                currentRole
                    .RoleType
                    .GetTradeInstance().IsSome;

            if (!isCurrentRoleTradeSpecific)
            {
                return new WorkflowResponse(
                    initialTradeUnknown.MyPrompt(),
                    glossary.GetId(typeof(NewIssueInitialTradeUnknown)));
            }
            
            var trade = currentRole.RoleType.GetTradeInstance().GetValueOrThrow();

            return trade.DividesLiveEventIntoSpheresOfAction switch
            {
                true => await SphereCanBeDeterminedFromUserLocationAsync() switch
                {
                    true => new WorkflowResponse(
                        initialSphereKnown.MyPrompt(),
                        glossary.GetId(typeof(NewIssueInitialSphereKnown))),
                    _ => new WorkflowResponse(
                        initialSphereUnknown.MyPrompt(),
                        glossary.GetId(typeof(NewIssueInitialSphereUnknown)))
                },

                _ => new WorkflowResponse(
                    sphereConfirmed.MyPrompt(),
                    glossary.GetId(typeof(NewIssueSphereConfirmed)))
            };

            async Task<bool> SphereCanBeDeterminedFromUserLocationAsync()
            {
                var lastKnownLocationInput = 
                    (await logicUtils.GetRecentLocationHistory(currentInput.TlgAgent))
                        .LastOrDefault();

                if (lastKnownLocationInput is null)
                    return false;

                var lastKnownLocation = 
                    lastKnownLocationInput.Details.GeoCoordinates.GetValueOrThrow();

                var liveEvent =
                    await liveEventsRepo.GetAsync(
                        currentInput.LiveEventContext.GetValueOrThrow());
                
                return
                    IsLocationNearSphere(
                        lastKnownLocation,
                        GetAllTradeSpecificSpheres(
                            liveEvent ?? throw new InvalidOperationException("LiveEvent missing."), 
                            trade),
                        trade);
            }

            static bool IsLocationNearSphere(
                Geo location,
                IReadOnlyCollection<ISphereOfAction> spheres,
                ITrade trade)
            {
                var tradeSpecificNearnessThreshold = trade switch
                {
                    SaniCleanTrade => SaniCleanTrade.SphereNearnessThresholdInMeters,
                    SiteCleanTrade => SiteCleanTrade.SphereNearnessThresholdInMeters,
                    _ => throw new InvalidOperationException("Missing switch for ITrade type for nearness threshold")
                };
                
                return
                    spheres
                        .Any(soa =>
                            soa.Details.GeoCoordinates.GetValueOrThrow()
                                .MetersAwayFrom(location) < tradeSpecificNearnessThreshold);
            }

            static IReadOnlyCollection<ISphereOfAction> 
                GetAllTradeSpecificSpheres(LiveEvent liveEvent, ITrade trade) => 
                liveEvent
                    .DivIntoSpheres
                    .Where(soa => soa.GetTradeType() == trade.GetType())
                    .ToImmutableReadOnlyCollection();
    }
}