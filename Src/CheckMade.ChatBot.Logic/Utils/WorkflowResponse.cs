using CheckMade.ChatBot.Logic.Workflows;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Utils;

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
            NewStateId: newState.Glossary.GetId(newState.GetType().GetInterfaces()[0]),
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
                UpdateExistingOutputMessageId = nextPromptInPlaceUpdateMessageId
            }
        ]; 
        
        if (currentPromptFinalizer.IsSome)
            outputs = outputs.Prepend(currentPromptFinalizer.GetValueOrThrow()).ToList();

        if (additionalOutputs != null)
            outputs.AddRange(additionalOutputs);
        
        return new WorkflowResponse(
            Output: outputs,
            NewStateId: newState?.Glossary.GetId(newState.GetType().GetInterfaces()[0]) ?? Option<string>.None(), 
            EntityGuid: entityGuid ?? Option<Guid>.None()
        );
    }
    
    private static (Option<int> nextPromptInPlaceUpdateMessageId, Option<OutputDto> currentPromptFinalizer)
        ResolvePromptTransitionIntoComponents(
            PromptTransition? promptTransition, 
            TlgInput currentInput)
    {
        var nextPromptInPlaceUpdateMessageId = promptTransition != null
            ? promptTransition.IsNextPromptInPlaceUpdate
                ? currentInput.TlgMessageId
                : Option<int>.None()
            : Option<int>.None();

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
        
        return new WorkflowResponse(
            Output: await newState.GetPromptAsync(
                currentInput,
                nextPromptInPlaceUpdateMessageId,
                currentPromptFinalizer),
            NewStateId: newState.Glossary.GetId(newState.GetType().GetInterfaces()[0]),
            EntityGuid: entityGuid ?? Option<Guid>.None());
    }

    internal static WorkflowResponse CreateWarningUseInlineKeyboardButtons(IWorkflowStateNormal currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please answer only using the buttons above.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType().GetInterfaces()[0]),
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
            NewStateId: currentState.Glossary.GetId(currentState.GetType().GetInterfaces()[0]),
            EntityGuid: Option<Guid>.None());

    internal static WorkflowResponse CreateWarningEnterTextOrAttachmentsOnly(IWorkflowStateNormal currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please enter a text message or a photo/file attachment only.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType().GetInterfaces()[0]),
            EntityGuid: Option<Guid>.None());
}