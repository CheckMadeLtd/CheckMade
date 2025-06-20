using CheckMade.Core.Model.Bot.DTOs.Input;

namespace CheckMade.Core.Model.Bot.DTOs.Output;

public readonly struct ActualSendOutParams
{
    public MessageId MessageId { get; init; }
    public ChatId ChatId { get; init; }
}