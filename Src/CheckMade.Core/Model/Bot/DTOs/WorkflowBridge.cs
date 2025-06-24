using CheckMade.Core.Model.Bot.DTOs.Inputs;

namespace CheckMade.Core.Model.Bot.DTOs;

/// <summary>
/// Creates a logical link between a workflow-terminating input from role A (e.g. submission) 
/// and a resulting message to role B (e.g. new submission notification). Thanks to such link, role B can e.g.
/// start a new workflow that relates to the original workflow (e.g. a submission), such as viewing attachments or
/// accepting it as a task.  
/// </summary>
public sealed record WorkflowBridge(
    Input SourceInput,
    ChatId DestinationChatId,
    MessageId DestinationMessageId);