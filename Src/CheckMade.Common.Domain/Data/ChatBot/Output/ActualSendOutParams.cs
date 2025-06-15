using CheckMade.Common.Domain.Data.ChatBot.Input;

namespace CheckMade.Common.Domain.Data.ChatBot.Output;

public readonly struct ActualSendOutParams
{
    public TlgMessageId TlgMessageId { get; init; }
    public ChatId ChatId { get; init; }
}