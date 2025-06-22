namespace CheckMade.Core.Model.Bot.DTOs.Input;

public sealed record HistoricInputIdentifier(
    MessageId HistoricMessageId, 
    DateTimeOffset HistoricTimeStamp);