using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows;

internal abstract record WorkflowBase(
    IGeneralWorkflowUtils WorkflowUtils,    
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IDomainGlossary Glossary) 
{
    protected internal async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        var allBridges = 
            await WorkflowUtils.GetWorkflowBridgesOrNoneAsync(currentInput.LiveEventContext);

        if (currentInput.IsWorkflowLauncher(allBridges))
            return await InitializeAsync(currentInput);
        
        var currentStateTypeOption = 
            await GetPreviousResultantStateTypeAsync(currentInput);

        if (currentStateTypeOption.IsNone)
        {
            return new WorkflowResponse(
                new OutputDto
                {
                    Text = Ui("Enter /start to begin.")
                },
                Option<string>.None());
        }

        var currentStateType = currentStateTypeOption.GetValueOrThrow();

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
    
    private async Task<Option<Type>> GetPreviousResultantStateTypeAsync(TlgInput currentInput)
    {
        var interactiveHistory =
            await WorkflowUtils.GetInteractiveWorkflowHistoryAsync(currentInput);

        if (interactiveHistory.Count <= 1)
            return Option<Type>.None();

        var lastInput =
            interactiveHistory
                .SkipLast(1)
                .Last();

        if (lastInput.ResultantWorkflow.IsNone)
            return Option<Type>.None();

        return
            Glossary.GetDtType(
                lastInput
                    .ResultantWorkflow.GetValueOrThrow()
                    .InStateId);
    }
}