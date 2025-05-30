using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows.Utils;

internal sealed record WorkflowResponse(
    IReadOnlyCollection<OutputDto> Output,
    Option<string> NewStateId,
    Option<Guid> EntityGuid)
{
    internal WorkflowResponse(
        OutputDto singleOutput, Option<string> newStateId, Guid? entityGuid = null) 
        : this(
            Output: new List<OutputDto> { singleOutput }, 
            NewStateId: newStateId,
            EntityGuid: entityGuid ?? Option<Guid>.None())
    {
    }
    
    internal WorkflowResponse(
        OutputDto singleOutput, IWorkflowStateNormal newState, Guid? entityGuid = null) 
        : this(
            Output: new List<OutputDto> { singleOutput }, 
            NewStateId: newState.Glossary.GetIdForEquallyNamedInterface(newState.GetType()),
            EntityGuid: entityGuid ?? Option<Guid>.None())
    {
    }

    internal static WorkflowResponse Create(
        TlgInput currentInput, 
        OutputDto singleOutput, 
        IReadOnlyCollection<OutputDto>? additionalOutputs = null,
        IWorkflowState? newState = null, 
        PromptTransition? promptTransition = null,
        Guid? entityGuid = null)
    {
        var (nextPromptInPlaceUpdateMessageId, currentPromptFinalizer) = 
            ResolvePromptTransitionIntoComponents(promptTransition, currentInput);

        List<OutputDto> outputs = 
        [
            singleOutput with
            {
                UpdateExistingOutputMessageId = nextPromptInPlaceUpdateMessageId,
                
                // Allows controlling the CallbackQuery Feedback message
                CallbackQueryId = currentInput.CallbackQueryId
            }
        ]; 
        
        if (currentPromptFinalizer.IsSome)
            outputs = outputs.Prepend(currentPromptFinalizer.GetValueOrThrow()).ToList();

        if (additionalOutputs != null)
            outputs.AddRange(additionalOutputs);
        
        return new WorkflowResponse(
            Output: outputs,
            NewStateId: newState?.Glossary.GetIdForEquallyNamedInterface(newState.GetType()) 
                        ?? Option<string>.None(), 
            EntityGuid: entityGuid ?? Option<Guid>.None()
        );
    }
    
    private static (Option<TlgMessageId> nextPromptInPlaceUpdateMessageId, Option<OutputDto> currentPromptFinalizer)
        ResolvePromptTransitionIntoComponents(
            PromptTransition? promptTransition, 
            TlgInput currentInput)
    {
        var nextPromptInPlaceUpdateMessageId = promptTransition != null
            ? promptTransition.IsNextPromptInPlaceUpdate
                ? currentInput.TlgMessageId
                : Option<TlgMessageId>.None()
            : Option<TlgMessageId>.None();

        var currentPromptFinalizer = promptTransition != null
            ? promptTransition.CurrentPromptFinalizer
            : Option<OutputDto>.None();

        return (nextPromptInPlaceUpdateMessageId, currentPromptFinalizer);
    }

    internal static async Task<WorkflowResponse> CreateFromNextStateAsync(
        TlgInput currentInput,
        IWorkflowStateNormal newState,
        PromptTransition? promptTransition = null,
        Guid? entityGuid = null)
    {
        var (nextPromptInPlaceUpdateMessageId, currentPromptFinalizer) = 
            ResolvePromptTransitionIntoComponents(promptTransition, currentInput);

        // Allows controlling the CallbackQuery Feedback message
        var promptFinalizerWithCallbackQueryId = currentPromptFinalizer.IsSome
            ? currentPromptFinalizer.GetValueOrThrow() with
            {
                CallbackQueryId = currentInput.CallbackQueryId
            }
            : currentPromptFinalizer;
        
        return new WorkflowResponse(
            Output: await newState.GetPromptAsync(
                currentInput,
                nextPromptInPlaceUpdateMessageId,
                promptFinalizerWithCallbackQueryId),
            NewStateId: newState.Glossary.GetIdForEquallyNamedInterface(newState.GetType()),
            EntityGuid: entityGuid ?? Option<Guid>.None());
    }

    internal static WorkflowResponse CreateWarningUseInlineKeyboardButtons(
        IWorkflowStateNormal currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please answer only using the buttons above.")
                }
            },
            NewStateId: currentState.Glossary.GetIdForEquallyNamedInterface(currentState.GetType()),
            EntityGuid: Option<Guid>.None());

    internal static WorkflowResponse
        CreateWarningChooseReplyKeyboardOptions(
            IWorkflowStateNormal currentState, IReadOnlyCollection<string> choices) => 
        new(Output: new List<OutputDto> 
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please choose from the options shown below."),
                    PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(choices)
                }
            },
            NewStateId: currentState.Glossary.GetIdForEquallyNamedInterface(currentState.GetType()),
            EntityGuid: Option<Guid>.None());

    internal static WorkflowResponse CreateWarningEnterTextOnly(IWorkflowStateNormal currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please only enter a text message.")
                }
            },
            NewStateId: currentState.Glossary.GetIdForEquallyNamedInterface(currentState.GetType()),
            EntityGuid: Option<Guid>.None());
}