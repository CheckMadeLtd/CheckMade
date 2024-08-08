using CheckMade.ChatBot.Logic.Utils;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Notifications;

internal sealed record ViewAttachmentsWorkflow(
    IGeneralWorkflowUtils GeneralWorkflowUtils,   
    IStateMediator Mediator) 
    : WorkflowBase(GeneralWorkflowUtils, Mediator)
{
    protected override Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        throw new NotImplementedException();
    }
}