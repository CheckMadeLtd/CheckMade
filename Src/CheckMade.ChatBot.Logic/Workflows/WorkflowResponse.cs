using CheckMade.Common.Interfaces.ChatBot.Logic;
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
    
    internal WorkflowResponse(OutputDto singleOutput, Type newStateType, IDomainGlossary glossary) 
        : this(
            Output: new List<OutputDto>{ singleOutput }, 
            NewStateId: glossary.GetId(newStateType))
    {
    }

    internal static async Task<WorkflowResponse> CreateAsync(IWorkflowState newState) =>
        new(Output: await newState.GetPromptAsync(),
            NewStateId: newState.Glossary.GetId(newState.GetType()));

    internal static WorkflowResponse CreateOnlyUseButtonsResponse(IWorkflowState currentState) =>
        new(Output: new List<OutputDto>
            {
                new()
                {
                    Text = Ui("Please answer only using the buttons above.")
                }
            },
            NewStateId: currentState.Glossary.GetId(currentState.GetType()));
}