using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow
{
    NewIssueWorkflow.States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory);
}

internal class NewIssueWorkflow : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyCollection<OutputDto>>> GetResponseAsync(TlgInput currentInput)
    {
        // get workflowinputHistory and locationHistory separately
        
        throw new NotImplementedException();
    }

    public States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInteractiveHistory,
        IReadOnlyCollection<TlgInput> recentLocationHistory)
    {
        var lastInteractiveInput = workflowInteractiveHistory.Last();

        if (IsBeginningOfWorkflow())
        {
            return CanDetermineSphereOfActionLocation() switch
            {
                true => States.InitialSphereKnown,
                _ => States.InitialSphereUnknown
            };
        }

        throw new InvalidOperationException($"Current State for {nameof(NewIssueWorkflow)} couldn't be determined.");
        
        bool IsBeginningOfWorkflow() => lastInteractiveInput.InputType == TlgInputType.CommandMessage;

        bool CanDetermineSphereOfActionLocation()
        {
            if (recentLocationHistory.Count == 0)
                return false;
             
            // ToDo: then compare the last (!!!) location update to the saved locations of spheres. 
            // if it's not further than the threshold defined for the corresponding TradeType then return true! 
            return true;
        }
    }

    [Flags]
    internal enum States
    {
        InitialSphereKnown = 1,
        InitialSphereUnknown = 1<<1,
        
        SphereConfirmed = 1<<2,
        
        IssueTypeInventorySelected = 1<<3,
        IssueTypeCleanlinessSelected = 1<<4,
        
        ReadyForEvidenceEntry = 1<<5,
        
        AwaitingReviewResults = 1<<6,
        EditingDetails = 1<<7,
        Completed = 1<<8
    }    
}