using CheckMade.Abstract.Domain.Data.Bot.Input;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal sealed record HistoricInputIdentifier(UserId HistoricUserId, DateTimeOffset HistoricTimeStamp);