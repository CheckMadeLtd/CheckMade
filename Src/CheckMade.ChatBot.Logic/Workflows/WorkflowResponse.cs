using CheckMade.Common.Model.ChatBot.Output;

namespace CheckMade.ChatBot.Logic.Workflows;

public record WorkflowResponse(
    IReadOnlyCollection<OutputDto> Output, 
    Option<Enum> NewState);