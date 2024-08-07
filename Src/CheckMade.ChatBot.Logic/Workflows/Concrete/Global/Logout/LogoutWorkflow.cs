using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;
using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;

internal interface ILogoutWorkflow : IWorkflow;

internal sealed record LogoutWorkflow(
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator) 
    : WorkflowBase(GeneralWorkflowUtils, Mediator), ILogoutWorkflow
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        return await WorkflowResponse.CreateFromNextStateAsync(
            currentInput,
            Mediator.Next(typeof(ILogoutWorkflowConfirm)));
    }
}