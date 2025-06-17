using CheckMade.Abstract.Domain.Data.ChatBot.Input;
using CheckMade.Abstract.Domain.Data.ChatBot.Output;
using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction.BotCommands;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Bot.Workflows.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

namespace CheckMade.Bot.Workflows.Workflows;

public abstract record WorkflowBase(
    IGeneralWorkflowUtils WorkflowUtils,    
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IDomainGlossary Glossary)
{
    internal static readonly UiString BeginWithStart = Ui("Enter {0} to begin.", Start.Command);
    
    protected internal async Task<Result<WorkflowResponse>> GetResponseAsync(Input currentInput)
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
                new Output
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
                new Output { Text = IGeneralWorkflowUtils.WorkflowWasCompleted },
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

            if (lastInput.ResultantState.IsNone)
                return Option<Type>.None();

            return
                Glossary.GetDtType(
                    lastInput
                        .ResultantState.GetValueOrThrow()
                        .InStateId);
        }
    }

    protected abstract Task<Result<WorkflowResponse>> InitializeAsync(Input currentInput);
}