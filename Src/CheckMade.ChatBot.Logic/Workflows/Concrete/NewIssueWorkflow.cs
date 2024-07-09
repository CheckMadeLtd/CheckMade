using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

using static NewIssueWorkflow.States;

internal interface INewIssueWorkflow : IWorkflow
{
    NewIssueWorkflow.States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory,
        TlgInput? currentInput);
}

internal class NewIssueWorkflow : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public Task<Result<(IReadOnlyCollection<OutputDto> Output, Option<long> NewState)>> 
        GetResponseAsync(TlgInput currentInput)
    {
        // get workflowinputHistory and locationHistory separately
        
        throw new NotImplementedException();
    }

    public States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory,
        TlgInput? currentInput)
    {
        // var lastInteractiveInput = workflowInteractiveHistory.Last();

        // if (IsBeginningOfWorkflow())
        // {
        //     if (IsTradeKnown())
        //     {
        //         return CanDetermineSphereOfActionLocation() switch
        //         {
        //             true => States.Initial_SphereKnown,
        //             _ => States.Initial_SphereUnknown
        //         };
        //     }
        // }

        
        throw new InvalidOperationException($"Current State for {nameof(NewIssueWorkflow)} couldn't be determined.");
        
        // bool IsBeginningOfWorkflow() => lastInteractiveInput.InputType == TlgInputType.CommandMessage;
        //
        // bool CanDetermineSphereOfActionLocation()
        // {
        //     if (recentLocationHistory.Count == 0)
        //         return false;
        //      
        //     // ToDo: then compare the last (!!!) location update to the saved locations of spheres. 
        //     // if it's not further than the threshold defined for the corresponding TradeType then return true! 
        //     return true;
        // }
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