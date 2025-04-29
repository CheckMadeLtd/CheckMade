using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.Utils;

namespace CheckMade.ChatBot.Logic.Workflows;

internal abstract record WorkflowBase(
    IGeneralWorkflowUtils WorkflowUtils,    
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IDomainGlossary Glossary)
{
    internal static readonly UiString BeginWithStart = Ui("Enter {0} to begin.", TlgStart.Command);
    
    protected internal async Task<ResultOld<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        var allBridges = 
            await WorkflowUtils.GetWorkflowBridgesOrNoneAsync(currentInput.LiveEventContext);

        if (currentInput.IsWorkflowLauncher(allBridges))
            return await InitializeAsync(currentInput);
        
        var currentStateTypeOption = 
            await GetPreviousResultantStateTypeAsync();

        /* The main scenario where this can happen is text entry after Logout, because of the unique combi of 3 factors:
         * 1. Lack of usual 'WorkflowLauncher' like botCommand and thus not launching InitializeAsync here
         * 2. There naturally is no ResultnatWorkflowState yet
         * 3. But the WorkflowIdentifier still returns UserAuthWorkflow because user is not authenticated
         */ 
        if (currentStateTypeOption.IsNone)
        {
            return new WorkflowResponse(
                new OutputDto
                {
                    Text = BeginWithStart
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
        
        async Task<Option<Type>> GetPreviousResultantStateTypeAsync()
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

    protected abstract Task<ResultOld<WorkflowResponse>> InitializeAsync(TlgInput currentInput);
}