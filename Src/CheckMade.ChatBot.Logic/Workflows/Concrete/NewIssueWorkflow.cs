using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow
{
    
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
}