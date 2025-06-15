using CheckMade.Common.Domain.Data.ChatBot.Input;

namespace CheckMade.Common.Domain.Data.ChatBot.Output;

public readonly struct ActualSendOutParams
{
    public MessageId MessageId { get; init; }
    public ChatId ChatId { get; init; }
}