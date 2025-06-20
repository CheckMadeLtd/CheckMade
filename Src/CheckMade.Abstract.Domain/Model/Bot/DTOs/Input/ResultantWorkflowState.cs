namespace CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;

public sealed record ResultantWorkflowState(
    string WorkflowId,
    string InStateId);