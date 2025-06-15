using CheckMade.Common.Domain.Data.ChatBot.Input;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal sealed record HistoricInputIdentifier(UserId HistoricUserId, DateTimeOffset HistoricTlgDate);