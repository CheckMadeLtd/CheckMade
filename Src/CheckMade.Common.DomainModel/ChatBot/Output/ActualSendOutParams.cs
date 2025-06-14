using CheckMade.Common.DomainModel.ChatBot.Input;

namespace CheckMade.Common.DomainModel.ChatBot.Output;

public readonly struct ActualSendOutParams
{
    public TlgMessageId TlgMessageId { get; init; }
    public TlgChatId ChatId { get; init; }
}