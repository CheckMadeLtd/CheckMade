using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;

internal interface IViewAttachmentsWorkflow : IWorkflow;

internal record ViewAttachmentsWorkflow : IViewAttachmentsWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException("ViewAttachments hahaha");
    }
}