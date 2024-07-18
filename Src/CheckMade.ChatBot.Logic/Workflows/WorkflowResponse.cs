using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public record WorkflowResponse(
    IReadOnlyCollection<OutputDto> Output,
    Option<string> NewState)
{
    internal WorkflowResponse(OutputDto singleOutput, Option<string> newStateId) 
    : this(
        Output: new List<OutputDto>{ singleOutput }, 
        NewState: newStateId)
    {
    }
    
    internal WorkflowResponse(OutputDto singleOutput, Type newStateType, IDomainGlossary glossary) 
        : this(
            Output: new List<OutputDto>{ singleOutput }, 
            NewState: glossary.GetId(newStateType))
    {
    }

    internal static async Task<WorkflowResponse> CreateAsync(IWorkflowState newState) =>
        new(Output: await newState.MyPromptAsync(),
            NewState: newState.Glossary.GetId(newState.GetType()));
}