namespace CheckMade.Core.Model.Bot.DTOs.Inputs;

public sealed record HistoricInputIdentifier(
    MessageId HistoricMessageId, 
    DateTimeOffset HistoricTimeStamp);