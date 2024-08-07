using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth.States;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using static CheckMade.Common.LangExt.InputValidator;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Global.UserAuth;

internal interface IUserAuthWorkflow : IWorkflow;

internal sealed record UserAuthWorkflow(
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator)
    : IUserAuthWorkflow
{
    internal static readonly OutputDto EnterTokenPrompt = new()
    {
        Text = Ui("🌀 Please enter your role token (format '{0}'): ", GetTokenFormatExample())
    };

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
                Mediator.Next(typeof(IUserAuthWorkflowTokenEntry)));
     
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