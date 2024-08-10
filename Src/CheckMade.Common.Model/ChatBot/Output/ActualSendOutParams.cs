using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.Common.Model.ChatBot.Output;

public struct ActualSendOutParams
{
    public int TlgMessageId { get; init; }
    public TlgChatId ChatId { get; init; }
}