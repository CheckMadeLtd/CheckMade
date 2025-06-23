using CheckMade.Core.Model.Bot.DTOs.Inputs;

namespace CheckMade.Core.Model.Bot.DTOs.Outputs;

public readonly struct ActualSendOutParams
{
    public MessageId MessageId { get; init; }
    public ChatId ChatId { get; init; }
}