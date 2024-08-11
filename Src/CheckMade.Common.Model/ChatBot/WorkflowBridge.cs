using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.Common.Model.ChatBot;

public sealed record WorkflowBridge(
    TlgInput SourceInput,
    TlgChatId DestinationChatId,
    TlgMessageId DestinationMessageId);