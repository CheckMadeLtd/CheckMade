using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

internal abstract record WorkflowBase(
    IGeneralWorkflowUtils GeneralWorkflowUtils,    
    IStateMediator Mediator) 
{
    protected internal async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        if (await GeneralWorkflowUtils.IsWorkflowLauncherAsync(currentInput))
            return await InitializeAsync(currentInput);
        
        var currentStateType = 
            await GeneralWorkflowUtils.GetPreviousResultantStateTypeAsync(currentInput);

        if (IsTerminatedWorkflow())
        {
            return WorkflowResponse.Create(
                currentInput,
                new OutputDto { Text = IGeneralWorkflowUtils.WorkflowWasCompleted },
                newState: Mediator.GetTerminator(currentStateType));
        }
        
        var currentState = Mediator.Next(currentStateType); 
        
        return await currentState.GetWorkflowResponseAsync(currentInput);

        bool IsTerminatedWorkflow() =>
            currentStateType.IsAssignableTo(typeof(IWorkflowStateTerminator));
    }

    protected abstract Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput);
}