using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;

internal interface IViewAttachmentsWorkflow : IWorkflow;

internal sealed record ViewAttachmentsWorkflow(
    IGeneralWorkflowUtils GeneralWorkflowUtils,   
    IStateMediator Mediator) 
    : WorkflowBase(GeneralWorkflowUtils, Mediator), IViewAttachmentsWorkflow
{
    protected override Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}