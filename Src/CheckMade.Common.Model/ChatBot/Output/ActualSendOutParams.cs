using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.Common.Model.ChatBot.Output;

public struct ActualSendOutParams
{
    public TlgMessageId TlgMessageId { get; init; }
    public TlgChatId ChatId { get; init; }
}