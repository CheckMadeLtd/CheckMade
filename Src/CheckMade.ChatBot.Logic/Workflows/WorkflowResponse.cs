using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public sealed record WorkflowResponse(
    IReadOnlyCollection<OutputDto> Output,
    Option<string> NewStateId)
{
    internal WorkflowResponse(OutputDto singleOutput, Option<string> newStateId) 
        : this(
            Output: new List<OutputDto>{ singleOutput }, 
            NewStateId: newStateId)
    {
    }
    
    internal WorkflowResponse(OutputDto singleOutput, IWorkflowState newState) 
        : this(
            Output: new List<OutputDto>{ singleOutput }, 
            NewStateId: newState.Glossary.GetId(newState.GetType().GetInterfaces()[0]))
    {
    }

    internal static async Task<WorkflowResponse> CreateAsync(
        TlgInput currentInput, IWorkflowState newState, bool editPreviousOutput = false) =>
        new(Output: await newState.GetPromptAsync(
                currentInput, 
                editPreviousOutput == false ? Option<int>.None() : currentInput.Details.TlgMessageId),
            NewStateId: newState.Glossary.GetId(newState.GetType().GetInterfaces()[0]));

    internal static WorkflowResponse CreateWarningUseInlineKeyboardButtons(IWorkflowState currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please answer only using the buttons above.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType().GetInterfaces()[0]));

    internal static WorkflowResponse
        CreateWarningChooseReplyKeyboardOptions(
            IWorkflowState currentState, IReadOnlyCollection<string> choices) => 
        new(Output: new List<OutputDto> 
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please choose from the options shown below."),
                    PredefinedChoices = Option<IReadOnlyCollection<string>>.Some(choices)
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType().GetInterfaces()[0]));

    internal static WorkflowResponse CreateWarningEnterTextOrAttachmentsOnly(IWorkflowState currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please enter a text message or a photo/file attachment only.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType().GetInterfaces()[0]));
}