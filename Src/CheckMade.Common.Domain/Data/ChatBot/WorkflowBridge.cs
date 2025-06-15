using CheckMade.Common.Domain.Data.ChatBot.Input;

namespace CheckMade.Common.Domain.Data.ChatBot;

/// <summary>
/// Creates a logical link between a workflow-terminating input from role A (e.g. submission) 
/// and a resulting message to role B (e.g. new submission notification). Thanks to such link, role B can e.g.
/// start a new workflow that relates to the original entity (e.g. submission), such as viewing attachments or
/// accepting it as a task.  
/// </summary>
public sealed record WorkflowBridge(
    Input.Input SourceInput,
    ChatId DestinationChatId,
    MessageId DestinationMessageId);