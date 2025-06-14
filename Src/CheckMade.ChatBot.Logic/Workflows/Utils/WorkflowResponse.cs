using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.ChatBot.Logic.Workflows.Utils;

public sealed record WorkflowResponse(
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
        Input currentInput, 
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
    
    private static (Option<MessageId> nextPromptInPlaceUpdateMessageId, Option<OutputDto> currentPromptFinalizer)
        ResolvePromptTransitionIntoComponents(
            PromptTransition? promptTransition, 
            Input currentInput)
    {
        var nextPromptInPlaceUpdateMessageId = promptTransition != null
            ? promptTransition.IsNextPromptInPlaceUpdate
                ? currentInput.MessageId
                : Option<MessageId>.None()
            : Option<MessageId>.None();

        var currentPromptFinalizer = promptTransition != null
            ? promptTransition.CurrentPromptFinalizer
            : Option<OutputDto>.None();

        return (nextPromptInPlaceUpdateMessageId, currentPromptFinalizer);
    }

    internal static async Task<WorkflowResponse> CreateFromNextStateAsync(
        Input currentInput,
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