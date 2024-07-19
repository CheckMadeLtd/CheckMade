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

    internal static async Task<WorkflowResponse> CreateAsync(IWorkflowState newState) =>
        new(Output: await newState.GetPromptAsync(),
            NewStateId: newState.Glossary.GetId(newState.GetType()));

    internal static WorkflowResponse CreateOnlyUseInlineKeyboardButtonResponse(IWorkflowState currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("❗️Invalid input! Please answer only using the buttons above.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType()));

    internal static WorkflowResponse
        CreateOnlyChooseReplyKeyboardOptionResponse(
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
}