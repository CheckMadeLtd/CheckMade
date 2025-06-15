using CheckMade.Common.DomainModel.Data.ChatBot.Input;

namespace CheckMade.Common.DomainModel.Data.ChatBot.Output;

public readonly struct ActualSendOutParams
{
    public TlgMessageId TlgMessageId { get; init; }
    public TlgChatId ChatId { get; init; }
}