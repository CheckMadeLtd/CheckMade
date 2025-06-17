using CheckMade.Abstract.Domain.Data.ChatBot.Input;

namespace CheckMade.Abstract.Domain.Data.ChatBot.Output;

public readonly struct ActualSendOutParams
{
    public MessageId MessageId { get; init; }
    public ChatId ChatId { get; init; }
}