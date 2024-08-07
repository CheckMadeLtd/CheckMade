using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting.States;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.LanguageSetting;

internal interface ILanguageSettingWorkflow : IWorkflow;

internal sealed record LanguageSettingWorkflow(
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator) 
    : ILanguageSettingWorkflow
{
    public async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
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
                Mediator.Next(typeof(ILanguageSettingSelect)));
     
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