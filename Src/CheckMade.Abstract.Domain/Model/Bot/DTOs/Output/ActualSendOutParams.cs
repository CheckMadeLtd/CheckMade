using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;

public readonly struct ActualSendOutParams
{
    public MessageId MessageId { get; init; }
    public ChatId ChatId { get; init; }
}