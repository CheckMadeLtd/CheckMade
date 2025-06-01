using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.Common.Model.ChatBot;

/// <summary>
/// Creates a logical link between a workflow-terminating input from role A (e.g. issue submission) 
/// and a resulting message to role B (e.g. new issue notification). Thanks to such link, role B can e.g.
/// start a new workflow that relates to the original entity (e.g. issue), such as viewing attachments or
/// accepting it as a task.  
/// </summary>
public sealed record WorkflowBridge(
    TlgInput SourceInput,
    TlgChatId DestinationChatId,
    TlgMessageId DestinationMessageId);