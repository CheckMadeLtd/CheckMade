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
        if (await IsVirginWorkflowAsync())
            return await InitializeAsync(currentInput);
        
        var currentStateType = 
            await GeneralWorkflowUtils.GetPreviousResultantStateTypeAsync(currentInput);

        if (IsTerminatedWorkflow())
        {
            return WorkflowResponse.Create(
                currentInput,
                new OutputDto { Text = IGeneralWorkflowUtils.WorkflowWasCompleted },
                newState: Mediator.Terminate(currentStateType));
        }
        
        var currentState = Mediator.Next(currentStateType); 
        
        return await currentState.GetWorkflowResponseAsync(currentInput);

        async Task<bool> IsVirginWorkflowAsync() =>
                (await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput))
                .Count == 1; // 1 because currentInput is already included

        bool IsTerminatedWorkflow() =>
            currentStateType.IsAssignableTo(typeof(IWorkflowStateTerminator));
    }

    protected abstract Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput);
}