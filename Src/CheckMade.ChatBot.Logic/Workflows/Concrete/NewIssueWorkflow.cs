using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow
{
    NewIssueWorkflow.States DetermineCurrentState(
        IReadOnlyCollection<TlgInput> workflowInputHistory);
}

internal class NewIssueWorkflow : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyCollection<OutputDto>>> GetResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }

    public States DetermineCurrentState(IReadOnlyCollection<TlgInput> workflowInputHistory)
    {
        throw new NotImplementedException();
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