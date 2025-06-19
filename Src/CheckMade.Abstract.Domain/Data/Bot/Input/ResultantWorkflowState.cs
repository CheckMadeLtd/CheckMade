namespace CheckMade.Abstract.Domain.Data.Bot.Input;

public sealed record ResultantWorkflowState(
    string WorkflowId,
    string InStateId);