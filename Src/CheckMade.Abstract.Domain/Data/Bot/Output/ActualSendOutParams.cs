using CheckMade.Abstract.Domain.Data.Bot.Input;

namespace CheckMade.Abstract.Domain.Data.Bot.Output;

public readonly struct ActualSendOutParams
{
    public MessageId MessageId { get; init; }
    public ChatId ChatId { get; init; }
}