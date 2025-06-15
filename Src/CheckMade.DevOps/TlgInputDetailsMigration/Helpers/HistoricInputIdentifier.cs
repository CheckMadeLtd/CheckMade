using CheckMade.Common.Domain.Data.ChatBot.Input;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal record HistoricInputIdentifier(UserId HistoricUserId, DateTimeOffset HistoricTlgDate);