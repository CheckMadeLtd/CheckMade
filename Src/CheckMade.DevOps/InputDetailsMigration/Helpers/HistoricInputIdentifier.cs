using CheckMade.Core.Model.Bot.DTOs.Input;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal sealed record HistoricInputIdentifier(UserId HistoricUserId, DateTimeOffset HistoricTimeStamp);