using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

internal abstract record WorkflowBase(
    IGeneralWorkflowUtils WorkflowUtils,    
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo) 
{
    protected internal async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        var allBridges = 
            await WorkflowUtils.GetWorkflowBridgesOrNoneAsync(currentInput.LiveEventContext);
        
        if (currentInput.IsWorkflowLauncher(allBridges))
            return await InitializeAsync(currentInput);
        
        var currentStateType = 
            await WorkflowUtils.GetPreviousResultantStateTypeAsync(currentInput);

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