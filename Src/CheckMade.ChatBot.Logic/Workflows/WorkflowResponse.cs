using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public record WorkflowResponse(
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
            NewStateId: newState.Glossary.GetId(newState.GetType()))
    {
    }

    internal static async Task<WorkflowResponse> CreateAsync(IWorkflowState newState, int? editMessageId = null) =>
        new(Output: await newState.GetPromptAsync(editMessageId ?? Option<int>.None()),
            NewStateId: newState.Glossary.GetId(newState.GetType()));

    internal static WorkflowResponse CreateWarningUseInlineKeyboardButtons(IWorkflowState currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please answer only using the buttons above.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType()));

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
            NewStateId: currentState.Glossary.GetId(currentState.GetType()));

    internal static WorkflowResponse CreateWarningEnterTextOrAttachmentsOnly(IWorkflowState currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please enter a text message or a photo/file attachment only.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType()));
}