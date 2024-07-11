using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;
using CheckMade.Common.Utils.GIS;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

using static NewIssueWorkflow.States;

internal interface INewIssueWorkflow : IWorkflow
{
    NewIssueWorkflow.States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory,
        LiveEvent currentLiveEvent);
}

internal class NewIssueWorkflow(
        ILiveEventsRepository liveEventsRepo,
        IDomainGlossary glossary) 
    : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<WorkflowResponse>> 
        GetResponseAsync(TlgInput currentInput)
    {
        // get WorkflowInputHistory and locationHistory separately
        
        // Here I will also need to switch on lastState! 
        // For example, if the lastState is SphereUnknown and the resulting state is again the same,
        // then an error message.
        // In other words, the info from which to which state we transitioned determines what prompt user sees next!
        
        throw new NotImplementedException();

        // var lifeEvent = await liveEventsRepo.GetAsync(currentInput.LiveEventContext.GetValueOrThrow());
    }

    public States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory,
        LiveEvent currentLiveEvent)
    {
        var currentInteractiveInput = workflowInteractiveHistory.Last();
        var currentRoleType = currentInteractiveInput.OriginatorRole.GetValueOrThrow().RoleType;

        if (IsBeginningOfWorkflow())
        {
            if (currentRoleType.GetTradeInstance().IsNone)
            {
                return Initial_TradeUnknown;
            }

            return CurrentTradeDividesLiveEventIntoSpheresOfAction() switch
            {
                true => CanDetermineSphereOfActionLocation() switch
                {
                    true => Initial_SphereKnown,
                    _ => Initial_SphereUnknown
                },

                _ => SphereConfirmed
            };
        }
        
        var lastInteractiveInput = workflowInteractiveHistory.SkipLast(1).Last();

        if (lastInteractiveInput.ResultantWorkflow.IsNone)
        {
            throw new InvalidOperationException($"Lack of '{nameof(ResultantWorkflowInfo)}' for last input processed " +
                                                $" by ({nameof(NewIssueWorkflow)}).");
        }

        if (lastInteractiveInput.ResultantWorkflow.GetValueOrThrow().WorkflowId !=
            glossary.IdAndUiByTerm[Dt(typeof(NewIssueWorkflow))].callbackId)
        {
            throw new InvalidOperationException($"WorkflowId of last Input unexpectedly is not for " +
                                                $"'{nameof(NewIssueWorkflow)}'");
        }

        var lastState = 
            (States)lastInteractiveInput
                .ResultantWorkflow.GetValueOrThrow()
                .InState;

        return lastState switch
        {
            Initial_SphereKnown => currentInteractiveInput.InputType switch
            {
                TlgInputType.CallbackQuery =>
                    currentInteractiveInput.Details.ControlPromptEnumCode.GetValueOrThrow() switch
                    {
                        (long)ControlPrompts.Yes => SphereConfirmed,
                        _ => Initial_SphereUnknown
                    },

                // e.g., someone entered a text instead of pressing a button -> no state change, instead "try again"
                _ => Initial_SphereKnown
            },
            
            Initial_SphereUnknown => currentInteractiveInput.InputType switch
            {
                TlgInputType.TextMessage => 
                    IsValidSphereOfActionName(
                            currentInteractiveInput.Details.Text.GetValueOrThrow()) switch 
                        {
                            true => SphereConfirmed,
                            _ => Initial_SphereUnknown
                        },
                
                _ => throw new InvalidOperationException("Nothing but free text entry should be possible at this State.")
            },
            
            _ => throw new InvalidOperationException($"Unhandled {nameof(lastState)}: '{lastState.ToString()}'")
        };
        
        bool IsBeginningOfWorkflow() => 
            currentInteractiveInput.InputType == TlgInputType.CommandMessage;

        bool CurrentTradeDividesLiveEventIntoSpheresOfAction() =>
            currentRoleType
                .GetTradeInstance().GetValueOrThrow()
                .DividesLiveEventIntoSpheresOfAction;
        
        bool CanDetermineSphereOfActionLocation()
        {
            var lastLocationUpdate = recentLocationHistory.LastOrDefault();
            
            if (lastLocationUpdate is null)
                return false;
            
            return 
                IsLocationNearASphere(
                    lastLocationUpdate,
                    GetAllSpheresForCurrentTrade());
        }

        IReadOnlyCollection<ISphereOfAction> GetAllSpheresForCurrentTrade() =>
            currentLiveEvent
                .DivIntoSpheres
                .Where(soa => 
                    soa.GetTradeType() 
                    == currentRoleType.GetTradeType().GetValueOrThrow())
                .ToImmutableReadOnlyCollection();
        
        static bool IsLocationNearASphere(
            TlgInput lastLocationUpdate,
            IReadOnlyCollection<ISphereOfAction> spheres)
        {
            var lastLocation = 
                lastLocationUpdate.Details.GeoCoordinates.GetValueOrThrow();

            return 
                spheres
                    .Any(soa => 
                        soa.Details.GeoCoordinates.GetValueOrThrow()
                            .MetersAwayFrom(lastLocation) < SaniCleanTrade.SphereNearnessThresholdInMeters);
        }

        bool IsValidSphereOfActionName(string textInput)
        {
            return
                GetAllSpheresForCurrentTrade()
                    .Select(soa => soa.Name)
                    .Contains(textInput);
        }
    }

    [Flags]
    internal enum States
    {
        Initial_TradeUnknown = 1,
        Initial_SphereKnown = 1<<1,
        Initial_SphereUnknown = 1<<2,
        
        SphereConfirmed = 1<<5,
        
        IssueType_SanitaryCleaning_Inventory_Selected = 1<<8,
        IssueType_SanitaryCleaning_WithFacilities_Selected = 1<<9,
        IssueType_WithoutFacilities_Selected = 1<<10,
        
        ReadyForEvidenceEntry = 1<<13,
        
        AwaitingReviewResults = 1<<16,
        EditingDetails = 1<<17,
        Completed = 1<<18
    }    
}