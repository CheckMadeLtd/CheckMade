using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public record WorkflowResponse(
    IReadOnlyCollection<OutputDto> Output,
    Option<string> NewState)
{
    internal WorkflowResponse(OutputDto singleOutput, Option<string> newState) 
    : this(Output: new List<OutputDto>{ singleOutput }, 
        NewState: newState)
    {
    }
}