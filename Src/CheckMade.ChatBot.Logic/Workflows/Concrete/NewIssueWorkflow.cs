using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow
{
    NewIssueWorkflow.States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory,
        LiveEvent liveEvent);
}

internal class NewIssueWorkflow(ILiveEventsRepository liveEventsRepo) : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<WorkflowResponse>> 
        GetResponseAsync(TlgInput currentInput)
    {
        // get workflowinputHistory and locationHistory separately
        
        throw new NotImplementedException();

        // var lifeEvent = await liveEventsRepo.GetAsync(currentInput.LiveEventContext.GetValueOrThrow();
    }

    public States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory,
        LiveEvent liveEvent)
    {
        var lastInteractiveInput = workflowInteractiveHistory.Last();
        var activeRoleType = lastInteractiveInput.OriginatorRole.GetValueOrThrow().RoleType;

        if (IsBeginningOfWorkflow())
        {
            if (activeRoleType.GetTrade().IsNone)
            {
                return States.Initial_TradeUnknown;
            }
            
            return CanDetermineSphereOfActionLocation() switch
            {
                true => States.Initial_SphereKnown,
                _ => States.Initial_SphereUnknown
            };
        }

        if (lastInteractiveInput.ResultantWorkflow.IsNone)
        {
            throw new InvalidOperationException($"Lack of '{nameof(ResultantWorkflowInfo)}' for last input processed " +
                                                $" by ({nameof(NewIssueWorkflow)}).");
        }
        
        // From here, base calc of State on combination of resultantState plus new input.
        
        throw new InvalidOperationException($"Current State for {nameof(NewIssueWorkflow)} couldn't be determined.");
        
        bool IsBeginningOfWorkflow() => 
            lastInteractiveInput.InputType == TlgInputType.CommandMessage;

        bool CanDetermineSphereOfActionLocation()
        {
            var lastLocationUpdate = recentLocationHistory.LastOrDefault();
            
            if (lastLocationUpdate is null)
                return false;

            var spheres = 
                liveEvent
                    .DivIntoSpheres
                    .Where(soa => soa.GetTrade() == activeRoleType.GetTrade().GetValueOrThrow())
                    .ToImmutableReadOnlyCollection();
            
            return IsLocationNearASphere(lastLocationUpdate, spheres);
        }

        static bool IsLocationNearASphere(
            TlgInput lastLocationUpdate,
            IReadOnlyCollection<ISphereOfAction> spheres)
        {
            throw new NotImplementedException();
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