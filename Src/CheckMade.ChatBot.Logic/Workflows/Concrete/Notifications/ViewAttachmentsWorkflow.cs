using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;

internal interface IViewAttachmentsWorkflow : IWorkflow;

internal sealed record ViewAttachmentsWorkflow(
        IStateMediator Mediator,
        IGeneralWorkflowUtils GeneralWorkflowUtils) 
    : IViewAttachmentsWorkflow
{
    public Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        throw new NotImplementedException("ViewAttachments hahaha");
    }
}