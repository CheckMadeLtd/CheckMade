using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout.States;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.Logout;

internal interface ILogoutWorkflow : IWorkflow;

internal sealed record LogoutWorkflow(
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator) 
    : ILogoutWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory) =>
        GeneralWorkflowUtils.IsWorkflowTerminated(inputHistory);

    public async Task<Result<WorkflowResponse>> 
        GetResponseAsync(TlgInput currentInput)
    {
        var interactiveHistory =
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);

        var lastInput =
            interactiveHistory
                .SkipLast(1) // skip currentInput
                .LastOrDefault();

        if (lastInput is null)
            return await WorkflowResponse.CreateFromNextStateAsync(
                currentInput,
                Mediator.Next(typeof(ILogoutWorkflowConfirm)));
     
        var currentStateType = 
            await GeneralWorkflowUtils.GetPreviousResultantStateTypeAsync(
                currentInput, 
                IGeneralWorkflowUtils.DistanceFromCurrentWhenRetrievingPreviousWorkflowState);

        if (currentStateType.IsAssignableTo(typeof(IWorkflowStateTerminator)))
        {
            return WorkflowResponse.Create(
                currentInput,
                new OutputDto { Text = IGeneralWorkflowUtils.WorkflowWasCompleted },
                newState: Mediator.Terminate(currentStateType));
        }
        
        var currentState = Mediator.Next(currentStateType); 
        
        return await currentState.GetWorkflowResponseAsync(currentInput);        
    }
}